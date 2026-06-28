# Language System 技術導讀

## Purpose

本文整理 MenthaAssembly / MenthaFlow 目前的多國語言系統行為，讓未來開發或 Agent 可以直接依照本文：

- 撰寫 `.lgp` 語言包。
- 判斷 `.lgp` 是否可使用 zip 壓縮格式。
- 在 WPF XAML 使用 `LanguageExtension`。
- 建立語言選擇下拉選單並切換 `LanguageManager.Custom`。

本文只描述目前架構，不修正既有 API 或行為。

## Overview

多國語言系統主要由三個部分組成：

- `LanguageManager`
  - 管理可用語言包、目前語言包、系統語言 fallback 與翻譯查詢。
- `LanguagePacket`
  - 讀取 `.lgp` 檔案，保存 key/value 內容，並支援合併。
- `LanguageExtension`
  - WPF `MarkupExtension`，讓 XAML 可以用 `{Language ...}` 綁定翻譯文字。

基本查詢流程為：

```text
LanguageManager.Translate(key, default)
    -> LanguageManager.Custom[key]
    -> LanguageManager.System[key]
    -> GoogleTranslate
    -> default
    -> key
```

## Language Packet Discovery

`LanguageManager.LanguagesFolder` 預設為：

```csharp
Path.Combine(Environment.CurrentDirectory, "Languages")
```

`LanguageManager.Languages` 只會列舉該資料夾底下的外層 `*.lgp` 檔案：

```csharp
Directory.EnumerateFiles($"*{ExtensionName}")
```

其中 `LanguageManager.ExtensionName` 預設為：

```text
.lgp
```

因此：

- 外層語言包檔案必須使用 `.lgp` 副檔名。
- 如果只把 `.lgp` 放在專案 `Resources` 資料夾，執行時不會自動被 `LanguageManager.Languages` 找到。
- 若要讓 UI 語言下拉選單看到語言包，需要將 `.lgp` 放到執行目錄的 `Languages` 資料夾，或調整 `LanguageManager.LanguagesFolder`。

## .lgp Text Format

`.lgp` 可以是純文字檔。每一行是一筆 key/value：

```text
Key=Value
```

範例：

```text
%CultureCode=en-US
Common_Menu_Label_File=File
Common_Menu_Label_Edit=Edit
Common_Menu_Label_Save=Save
```

格式規則：

- `%` 開頭為 command。
- `//` 開頭為註解。
- 一般內容以第一個 `=` 分隔 key 與 value。
- value 內不可放實際換行，需使用 `\r` / `\n`。
- 讀取時會將 `\r` / `\n` 還原成實際換行。

多行文字範例：

```text
Sample_Description=Line 1\r\nLine 2
```

### CultureCode

語言文化代碼使用 command：

```text
%CultureCode=en-US
```

command name 會以 `ToLower()` 比對，因此大小寫不敏感：

```text
%CultureCode=en-US
%culturecode=en-US
%CULTURECODE=en-US
```

但拼字必須正確。應使用 `CultureCode`，不要使用 `CultrueCode`。

目前 `LanguagePacket.Save()` 內寫出的 command 是：

```text
%CultrueCode=
```

這是既有拼字問題；手動撰寫語言包時仍應使用正確的 `%CultureCode=`。

## .lgp Zip Format

`.lgp` 也可以是 zip 壓縮檔。`LanguagePacket.Parse(Stream)` 會先用 `ArchiveHelper.IsZipArchive(Stream)` 判斷內容是否為 zip：

```text
PK 03 04
```

如果外層 `.lgp` 是 zip，會讀取所有 `ZipArchiveEntry`：

```csharp
foreach (ZipArchiveEntry Entry in Archive.Entries.OrderBy(i => i.LastWriteTime))
{
    Stream Content = Entry.Open();
    Parse(Content);
}
```

重要行為：

- 外層檔案仍需 `.lgp`，因為 `LanguageManager.Languages` 只掃描 `*.lgp`。
- zip 內 entry 不需要 `.lgp` 副檔名。
- zip 內每個 entry 都會被解析。
- entry 若本身也是 zip，會遞迴解析。
- entry 若不是 zip，就會被視為純文字 `.lgp` 片段。

建議 zip 內部結構：

```text
Common.lgp
LaunchWindow.lgp
MainWindow.lgp
SolutionExplorerView.lgp
Secs/ConnectionSubView.lgp
Secs/VariablesSubView.lgp
Secs/RecipeSchemaSubView.lgp
```

雖然 entry 不需要副檔名，但建議使用 `.lgp` 或 `.txt`，方便人工檢視。

## Merge Behavior

### Zip 內自然合併

zip `.lgp` 內的 entries 會依 `LastWriteTime` 排序解析。

當多個 entry 定義相同 key 時，後解析的值會覆蓋前面的值：

```csharp
Contexts[Key] = Value;
```

這可用於分層語言包，例如：

```text
00_Common.lgp
10_MainWindow.lgp
90_ProjectOverride.lgp
```

### TryMerge

`LanguagePacket.TryMerge(LanguagePacket packet, bool override)` 可手動合併兩個語言包。

行為：

- 若來源 packet 有 `CultureCode`：
  - 目前 packet 沒有 `CultureCode` 時，會採用來源的 `CultureCode`。
  - 目前 packet 已有不同 `CultureCode` 時，回傳 `false`。
- `override = true`：
  - 來源 key 覆蓋目前 key。
- `override = false`：
  - 只補上目前 packet 沒有的 key。

## WPF LanguageExtension

XAML 可直接使用 `LanguageExtension`：

```xaml
<TextBlock Text="{Language Common_Menu_Label_Save, Default='Save'}" />
<Button Content="{Language Common_Menu_Label_SaveAll, Default='Save All'}" />
<MenuItem Header="{Language Common_Menu_Label_File, Default='File'}" />
```

常見可用屬性：

- `TextBlock.Text`
- `Button.Content`
- `MenuItem.Header`
- `ToolTip`
- `TabItem.Header`
- `DataGridColumn.Header`

`LanguageExtension` 會建立 `MultiBinding`，監聽：

- `LanguageManager.EnableGoogleTranslate`
- `LanguageManager.Custom`
- 內部 proxy object

因此切換 `LanguageManager.Custom` 後，已綁定的 UI 文字會自動更新。

### 動態 key

`LanguageExtension.Source` 型別是 `BindingBase`，可讓 language key 由目前 `DataContext`、集合 item 或指定屬性提供。

固定 key 用法：

```xaml
<TextBlock Text="{Language Common_Menu_Label_Save, Default='Save'}" />
```

動態 key 可直接使用目前 item：

```xaml
<TextBlock Text="{Language Source={Binding}}" />
```

也可以指定目前 item 的某個屬性作為 key：

```xaml
<MenuItem Header="{Language Source={Binding HeaderLanguageKey}}" />
```

集合搭配 `DataTemplate` 時，最常見的寫法是讓每個 item 提供自己的 language key：

```xaml
<ItemsControl ItemsSource="{Binding Items}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Language Source={Binding LanguageKey}}" />
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

使用 `Source` 時仍可提供 `Default`：

```xaml
<TextBlock Text="{Language Source={Binding LanguageKey}, Default='Unknown'}" />
```

fallback 行為：

- `Source` binding 回傳 `DependencyProperty.UnsetValue` 時，回傳 `null`。
- `Source` binding 回傳空值時，優先使用 `Default`。
- `Default` 也為空時，會回傳 `Binding.DoNothing` 或 key fallback。

C# 動態建立 binding 時可使用：

```csharp
LanguageExtension.Create(path, defaultText)
```

### Code-behind 翻譯

非 XAML 場景可使用：

```csharp
string text = LanguageManager.Translate("Common_Menu_Label_Save", "Save");
```

### DataTrigger Setter 注意事項

在 WPF `Setter.Value` 中直接使用 `{Language ...}` 可能遇到 markup extension 解析限制。

若文字會依狀態切換，較穩定的做法是：

- 建立多個 `TextBlock`。
- 各自使用 `{Language ...}`。
- 用 `DataTrigger` 控制 `Visibility`。

範例：

```xaml
<Grid>
    <TextBlock Text="{Language Sample_Active_Description, Default='Active description.'}" />
    <TextBlock Text="{Language Sample_Passive_Description, Default='Passive description.'}">
        <TextBlock.Style>
            <Style TargetType="TextBlock">
                <Setter Property="Visibility" Value="Collapsed" />
                <Style.Triggers>
                    <DataTrigger Binding="{Binding IsPassive}" Value="True">
                        <Setter Property="Visibility" Value="Visible" />
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </TextBlock.Style>
    </TextBlock>
</Grid>
```

## Language Selector Binding

語言下拉選單可直接綁定 `LanguageManager.Languages`。

XAML 範例：

```xaml
<ComboBox ItemsSource="{Binding Source={x:Static assembly:LanguageManager.Languages}}"
          DisplayMemberPath="LanguageName"
          SelectedValuePath="LanguageName"
          SelectedValue="{Binding Language, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
```

ViewModel / settings model 範例：

```csharp
public string Language
{
    get => LanguageManager.Custom?.LanguageName;
    set
    {
        if (Language == value)
            return;

        LanguageManager.Custom = string.IsNullOrWhiteSpace(value)
            ? null
            : LanguageManager.Languages.FirstOrDefault(i => i.LanguageName == value);

        OnPropertyChanged();
    }
}
```

顯示文字來自：

```csharp
LanguagePacket.LanguageName
```

也就是 `.lgp` 檔名不含副檔名。

例如：

```text
English.lgp -> English
TraditionalChinese.lgp -> TraditionalChinese
```

## Build-Time Language Pack Workflow

專案建議不要直接維護輸出的 `.lgp` 檔案。更穩定的方式是把語言內容維護在可讀的 `.txt` 來源中，並在 build 時自動輸出 `.lgp`。

這種流程的核心原則：

- `Languages` 底下的 `.txt` 或語言資料夾是唯一需要人工維護的來源。
- `.txt` 內容仍使用標準 `.lgp` 文字格式。
- build 後產生的 `.lgp` 是 artifact，不應再手動編輯。
- 輸出位置維持 `$(TargetDir)/Languages`，讓 `LanguageManager.Languages` 可以直接載入。

### Single Text File Mode

小型專案若 key 數量少、不需要依 UI 區域拆分，可以直接維護單一 `.txt`。因為 `.lgp` 本身支援純文字格式，build 時只需要複製來源檔並改副檔名。

建議來源結構：

```text
Languages/
  English.txt
  TraditionalChinese.txt
```

`English.txt` 內容範例：

```text
%CultureCode=en-US
Common_Menu_Label_File=File
Common_Menu_Label_Edit=Edit
Common_Menu_Label_Save=Save
```

專案可在 `.csproj` 加入 build target，把每個 `.txt` 輸出成同名 `.lgp`：

> Single Text File Mode 與 Folder Zip Mode 的 build target 擇一使用即可。

```xml
<Target Name="BuildLanguagePacks" AfterTargets="Build" Condition="Exists('$(MSBuildProjectDirectory)\Languages')">
    <MakeDir Directories="$(TargetDir)Languages" />
    <ItemGroup>
        <LanguagePackTextFiles Include="$(MSBuildProjectDirectory)\Languages\*.txt" />
    </ItemGroup>
    <Copy SourceFiles="@(LanguagePackTextFiles)"
          DestinationFiles="@(LanguagePackTextFiles->'$(TargetDir)Languages\%(Filename).lgp')" />
</Target>
```

上述 target 的輸出範例：

```text
bin/Debug/net8.0-windows/Languages/English.lgp
bin/Debug/net8.0-windows/Languages/TraditionalChinese.lgp
```

### Folder Zip Mode

大型 WPF 專案若 key 數量較多，建議把每個語言包拆成資料夾，並依 UI 區域分成多個 `.txt`，再在 build 時自動壓縮成 `.lgp`。

建議來源結構：

```text
Languages/
  English/
    00_Common.txt
    10_LaunchWindow.txt
    20_MainWindow.txt
    30_SolutionExplorerView.txt
    50_SECS/
      ConnectionSubView.txt
      VariablesSubView.txt
      RecipeSchemaSubView.txt
    90_RuntimeUsedKeys.txt
  TraditionalChinese/
    00_Common.txt
    10_LaunchWindow.txt
    20_MainWindow.txt
    30_SolutionExplorerView.txt
    50_SECS/
      ConnectionSubView.txt
      VariablesSubView.txt
      RecipeSchemaSubView.txt
    90_RuntimeUsedKeys.txt
```

`00_Common.txt` 通常放置共用 key 與語系資訊：

```text
%CultureCode=en-US
Common_Menu_Label_File=File
Common_Menu_Label_Edit=Edit
Common_Menu_Label_Save=Save
```

專案可在 `.csproj` 加入 build target，把每個語言資料夾壓縮為同名 `.lgp`：

> Folder Zip Mode 與 Single Text File Mode 的 build target 擇一使用即可。

```xml
<Target Name="BuildLanguagePacks" AfterTargets="Build" Condition="Exists('$(MSBuildProjectDirectory)\Languages')">
    <MakeDir Directories="$(TargetDir)Languages" />
    <Exec Command="powershell -NoProfile -ExecutionPolicy Bypass -Command &quot;$ErrorActionPreference = 'Stop'; $sourceRoot = '$(MSBuildProjectDirectory)\Languages'; $targetRoot = '$(TargetDir)Languages'; New-Item -ItemType Directory -Force -Path $targetRoot | Out-Null; Get-ChildItem -LiteralPath $sourceRoot -Directory | ForEach-Object { $languageName = $_.Name; $tempZip = Join-Path $targetRoot ($languageName + '.zip'); $targetPacket = Join-Path $targetRoot ($languageName + '.lgp'); if (Test-Path $tempZip) { Remove-Item -LiteralPath $tempZip -Force }; if (Test-Path $targetPacket) { Remove-Item -LiteralPath $targetPacket -Force }; $items = Join-Path $_.FullName '*'; Compress-Archive -Path $items -DestinationPath $tempZip -Force; Move-Item -LiteralPath $tempZip -Destination $targetPacket -Force }&quot;" />
</Target>
```

上述 target 的輸出範例：

```text
bin/Debug/net8.0-windows/Languages/English.lgp
bin/Debug/net8.0-windows/Languages/TraditionalChinese.lgp
```

壓縮時應只包含語言資料夾內部內容，不包含外層語言資料夾本身。也就是 `English.lgp` 內部應直接看到：

```text
00_Common.txt
10_LaunchWindow.txt
20_MainWindow.txt
30_SolutionExplorerView.txt
50_SECS/ConnectionSubView.txt
```

Folder Zip Mode 新增語言時，只需要複製一份語言來源資料夾：

```text
Languages/English
Languages/TraditionalChinese
```

然後修改各 `.txt` 的 value 與 `%CultureCode`。下一次 build 會自動產生新的 `.lgp`。

這個流程仍可搭配 `Resources/LanguageKeys_*.md`。建議分工如下：

- `Resources/LanguageKeys_*.md`：記錄 key 命名、用途、UI 位置與 fallback 說明。
- `Languages/{LanguageName}.txt`：Single Text File Mode 的實際語言包內容。
- `Languages/{LanguageName}/*.txt`：Folder Zip Mode 的實際語言包內容。
- `$(TargetDir)/Languages/*.lgp`：build 產生的執行期語言包。

## Recommended Authoring Workflow

建議流程：

1. 在 XAML 固定字串上使用 `{Language Key, Default='English'}`。
2. 在 `Resources/LanguageKeys_*.md` 記錄 key、English fallback、用途與 UI element。
3. 依專案規模選擇 Single Text File Mode 或 Folder Zip Mode。
4. 小型專案在 `Languages/English.txt` 維護英文語言包來源。
5. 大型專案在 `Languages/English/*.txt` 維護英文語言包來源，並依 UI 區域拆分，例如 `00_Common.txt`、`20_MainWindow.txt`、`50_SECS/*.txt`。
6. 透過 build target 自動輸出 `$(TargetDir)/Languages/English.lgp`。
7. 新增其他語言時，複製對應的 `.txt` 或語言資料夾並翻譯 value。
8. 在 App Settings 的語言下拉選單選取語言包。
9. 驗證 UI 是否隨 `LanguageManager.Custom` 切換而更新。

## Pitfalls

- 若專案使用 build-time language pack workflow，請只維護 `Languages` 底下的來源 `.txt`，不要再手動維護輸出的 `.lgp`。
- Single Text File Mode 請只維護 `Languages/{LanguageName}.txt`，輸出時仍必須改成 `.lgp`，否則 `LanguageManager.Languages` 不會列舉。
- Single Text File Mode 適合 key 數量少、分區需求低的小專案；key 變多時可遷移到 Folder Zip Mode，因為兩者的 `.txt` 內容格式相同。
- build target 壓縮時應使用語言資料夾內部內容，例如 `Languages/English/*`，不要把 `English` 資料夾本身包進 zip。
- `LanguageManager.LanguagesFolder` 預設讀取執行目錄下的 `Languages`，輸出資料夾名稱應保持一致，除非專案明確改寫該路徑。

- 外層語言包必須是 `.lgp`，否則 `LanguageManager.Languages` 不會列舉。
- zip 內 entry 會全部解析，不要放非語言內容。
- 同 key 會被後解析的 entry 覆蓋；使用 zip 時需注意 entry `LastWriteTime`。
- `%CultureCode` 大小寫不敏感，但拼字必須正確。
- `LanguagePacket.Save()` 目前寫出 `%CultrueCode`，不建議用它作為手寫格式參考。
- value 中不要放實際換行，請寫成 `\r` / `\n`。
- XAML `Default` 應維持英文，作為沒有語言包時的 fallback。
- 不要翻譯 protocol identifier、binding path、resource key、command name 或 domain identifier。
