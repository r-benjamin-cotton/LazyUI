#ifndef LAZY_CG_INCLUDED
#define LAZY_CG_INCLUDED

#define TEXEL_OFFSET    (0.5)

half3 _toneCurve(half3 rgb, float value)
{
#if LAZY_TONE_RAW
    return rgb;
#elif LAZY_TONE_HARD
    return lerp(rgb, 0.5 - 0.5 * cos(rgb * 3.141592), value);
#elif LAZY_TONE_SOFT
    return lerp(rgb, acos(rgb * -2 + 1) * (1 / 3.14592), value);
#elif LAZY_TONE_HIGHKEY
    return lerp(rgb, pow(rgb, 0.5), value);
#elif LAZY_TONE_LOWKEY
    return lerp(rgb, pow(rgb, 2), value);
#else
    return rgb;
#endif
}

half4 _tex2DF(sampler2D tex, float2 uv, float lod)
{
    return tex2Dlod(tex, float4(uv, 0, lod));
}

half4 _tex2DL(sampler2D tex, float2 uv, float4 texelSize, float lod)
{
#if LAZY_ANTI_ALPHA_ON
    float2 ux = uv * texelSize.zw - TEXEL_OFFSET;
    float2 ui = floor(ux);
    float2 uf = ux - ui;
    uv = ui * texelSize.xy + 0.5 * texelSize.xy;
    float2 uv0 = uv;
    float2 uv1 = uv + texelSize.xy;
    half4 c00 = _tex2DF(tex, float2(uv0.x, uv0.y), lod);
    half4 c01 = _tex2DF(tex, float2(uv1.x, uv0.y), lod);
    half4 c10 = _tex2DF(tex, float2(uv0.x, uv1.y), lod);
    half4 c11 = _tex2DF(tex, float2(uv1.x, uv1.y), lod);
    c00.rgb *= c00.a;
    c01.rgb *= c01.a;
    c10.rgb *= c10.a;
    c11.rgb *= c11.a;
    half4 c0 = lerp(c00, c01, uf.x);
    half4 c1 = lerp(c10, c11, uf.x);
    half4 cc = lerp(c0, c1, uf.y);
    return cc;
#else
    return _tex2DF(tex, uv, lod);
#endif
}

#if 0
float2 FWidth(float2 uv, float4 texelSize)
{
    float2 ux = uv * texelSize.zw;
    float2 xx = ddx(ux);
    float2 yy = ddy(ux);
    float2 txx;
    float2 txy;
    float2 tyx;
    float2 tyy;
    if (abs(xx.y) > abs(yy.y))
    {
        txx = xx;
        txy = yy;
    }
    else
    {
        txx = yy;
        txy = xx;
    }
    if (abs(xx.x) > abs(yy.x))
    {
        tyx = xx;
        tyy = yy;
    }
    else
    {
        tyx = yy;
        tyy = xx;
    }
    float wx = abs(((txx.y == 0) ? txx.x : (txx.x * clamp(txy.y / txx.y, -1, +1))) - txy.x);
    float wy = abs(((tyx.x == 0) ? tyx.y : (tyx.y * clamp(tyy.x / tyx.x, -1, +1))) - tyy.y);
    return float2(wx, wy);
}
#elif 1
// 面積の平方根で推定
float FWidth(float2 uv, float4 texelSize)
{
    float2 ux = uv * texelSize.zw;
    float2 xx = ddx(ux);
    float2 yy = ddy(ux);
    float s = abs(xx.x * yy.y - xx.y * yy.x);
    float ll = sqrt(s);
    return ll;
}
#else
// 縦横で拡大率の小さいほうを採用
float FWidth(float2 uv, float4 texelSize)
{
    float2 ux = uv * texelSize.zw;
    float2 xx = ddx(ux);
    float2 yy = ddy(ux);
    float ll = sqrt(min(dot(xx, xx), dot(yy, yy)));
    return ll;
}
#endif
#if 0
float mipLevel(float2 v)
{
    float2 dxv = ddx(v);
    float2 dyv = ddy(v);
    float dms = max(dot(dxv, dxv), dot(dyv, dyv));
    return 0.5 * log2(dms);
}
#endif

half4 LazyTex2D(sampler2D tex, float2 uv, float4 texelSize)
{
    float fw = max(1, FWidth(uv, texelSize));
    float lod = log2(fw);
#if LAZY_FILTER_SOFT
    //float2 hp = texelSize.xy * 0.5;
    float2 hp = texelSize.xy * 0.5 * fw;
    float2 uv0 = uv - hp;
    float2 uv1 = uv + hp;
    half4 c00 = _tex2DL(tex, float2(uv0.x, uv0.y), texelSize, lod);
    half4 c01 = _tex2DL(tex, float2(uv1.x, uv0.y), texelSize, lod);
    half4 c10 = _tex2DL(tex, float2(uv0.x, uv1.y), texelSize, lod);
    half4 c11 = _tex2DL(tex, float2(uv1.x, uv1.y), texelSize, lod);
    return (c00 + c01 + c10 + c11) * 0.25;
#elif LAZY_FILTER_LINEAR
    uv = uv;
    return _tex2DL(tex, uv, texelSize, lod);
#elif LAZY_FILTER_COS
    float2 ux = uv * texelSize.zw + TEXEL_OFFSET;
    float2 ui = floor(ux);
    float2 uf = ux - ui;
    uv = ui * texelSize.xy - 0.5 * cos(uf * 3.141592) * texelSize.xy;
    return _tex2DL(tex, uv, texelSize, lod);
#elif LAZY_FILTER_EDGE
    float2 ux = uv * texelSize.zw + TEXEL_OFFSET;
    float2 ui = floor(ux);
    float2 uf = ux - ui;
#if 1
    float2 ddxuv = ddx(ux);
    float2 ddyuv = ddy(ux);
    float2 ll = sqrt(ddxuv * ddxuv + ddyuv * ddyuv);
    float2 scale = 1.0 / saturate(ll);
    uf = saturate(0.5 + (uf - 0.5) * scale);
#endif
    uv = ui * texelSize.xy - 0.5 * cos(uf * 3.141592) * texelSize.xy;
    return _tex2DL(tex, uv, texelSize, lod);
#elif LAZY_FILTER_POINT
    float2 ux = uv * texelSize.zw;
    float2 ui = floor(ux);
    uv = ui * texelSize.xy + 0.5 * texelSize.xy;
    half4 col = _tex2DF(tex, uv, lod);
    col.rgb *= col.a;
    return col;
#elif LAZY_FILTER_SHARP
#if 1
    float2 hp = texelSize.xy * fw * 0.5;
    float ll = lod;
#else // strong
    float2 hp = texelSize.xy * fw;
    float ll = lod + 1;
#endif
    float2 uv0 = uv - hp;
    float2 uv1 = uv + hp;
    half4 c00 = _tex2DL(tex, float2(uv0.x, uv0.y), texelSize, ll);
    half4 c01 = _tex2DL(tex, float2(uv1.x, uv0.y), texelSize, ll);
    half4 c10 = _tex2DL(tex, float2(uv0.x, uv1.y), texelSize, ll);
    half4 c11 = _tex2DL(tex, float2(uv1.x, uv1.y), texelSize, ll);
    half4 cxx = _tex2DL(tex, uv, texelSize, lod);
    half4 cvv = c00 + c01 + c10 + c11;
    return (cvv == 0) ? cxx : saturate(cxx * cxx / cvv * 4);
#else
    return tex2D(tex, uv);
#endif
}

#endif //LAZY_CG_INCLUDED
