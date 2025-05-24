# LazyUI
Unity用の手抜きUIパッケージです。\
UI要素から直接他のコンポーネントのプロパティを参照できます。\
UIを操作するコードを書かなくても簡単なUIを作成できます。

*Unity2022.3でのみ動作確認

# Install
UnityのPackageManagerで"Add package from git URL..."にて以下を指定。\
https://github.com/r-benjamin-cotton/LazyUI.git

# Sample
トグルやスライダーのデモプログラム。\
LazyUISampleTargetのプロパティを各コントロールから直接操作する。

パッケージマネージャーからインストール。\
シーンはSapmles/LazyUI/[version]/LazyUI Sample/LazyUISample。

スプライトフォントのための空のフォントで以下の警告が出るので\
"The character used for Underline is not available in font asset [Empty SDF]."

下記設定にチェックを入れる。\
Edit/Project Settings../TextMesh Pro/Settings/Dynamic Font System Settings/Disable warnings



# Dependencies
com.unity.ugui: 1.0.0\
com.unity.inputsystem: 1.7.0\
com.unity.textmeshpro: 3.0.6

# Usage
GameObjectメニューのLazyUIから要素を選んで配置。

TargetPropertyに操作対象のオブジェクト、コンポーネント、プロパティ、\
一部では値を指定する。

列挙型はインデックス値。

## UIコンポーネント
### PropertyRadioButton
ラジオボタン\
ターゲットの値が指定の値と一致した場合チェック状態に、\
非チェック状態でボタンを押すとターゲットに値を設定。

### PropertyRotarySwitch
多ステートスイッチ\
ターゲットの値に応じたイメージに切り替わる\
クリックすると+1した値をターゲットに設定。

### PropertySelector
選択コントロール\
ターゲットの列挙型を一覧から選択できる。

### PropertySlider
スライダー\
ターゲットの値を反映、\
スライドするとターゲットに値を設定。\
※対象のプロパティ名に"Range"がついたプロパティでRange<>型を返すとmin/maxが自動設定される。

### PropertyRangeSlider
範囲指定スライダー\
ターゲットの値を反映、\
スライドするとターゲットに値を設定。

### PropertySpinControl
指定プロパティを+1/-1する。

### PropertyText
文字列プロパティを反映

### PropertyInputField
文字列入力\
値を確定すると指定プロパティへ設定。


## その他コンポーネント
### LazyLayout
UIの子コンポーネントを水平化垂直方向へ整列する。

### QuickButton
ボタンを押した瞬間にclikを呼び出すボタンコントロール。\
リピート付き。

### PropertyCondition
指定した条件に一致した場合、指定した値をターゲットに書き込む。

### LazyShortcut
指定したキーの組み合わせで指定したアクション(メソッド)を実行。


## 機能集約クラス
### LazyCallbacker
UpdateやLateUpdateなどのイベントを集約し優先度付けたりしてみる常駐型コンポーネント。

### LazyDebug
デバッグログをファイルへ書き込む。

### LazyPlayerPrefs
Windowsでプライヤー設定をiniファイルへ書き込む。

## 他
省略

# Material
ピクセルを強調したい時に使うUI用マテリアルを同梱。


