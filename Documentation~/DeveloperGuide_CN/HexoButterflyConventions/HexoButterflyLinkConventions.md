# Hexo Butterfly 链接格式规范

对于 Obsidian 中的链接引用，都将通过函数 [`HexoUtils.ConvertObsidianLinkToHexo`](xref:Obsidian2.Utilities.Hexo.HexoUtils.ConvertObsidianLinkToHexo*) 将其转换为 Hexo Butterfly 主题支持的格式。其主要将处理：

- [文档链接转换](#文档链接转换)，诸如 `[Link Text](Relative/Path/To/Document.md)` 的链接
- [同文档内锚点链接转换](#同文档内锚点链接转换)，诸如 `[See Section](#Section-Title)` 的链接
- [跨文档锚点链接转换](#跨文档锚点链接转换)，诸如 `[See Section](OtherDocument.md#Section-Title)` 的链接
- [资源文件链接转换](#资源文件链接转换)，诸如 `![Image](Relative/Path/To/Image.png)` 的链接


## 文档链接转换

因为基于 Hexo 平级目录结构（所有的 .md 都在同 `_posts` 目录下（见 [HexoButterflyFolderConventions](HexoButterflyFolderConventions.md)），链接转换会应用以下规则：

- 移除目录结构，直接使用文件名
- **中文字符转换为拼音**（如：`我的文章.md` → `wo_de_wen_zhang.md`）
- 应用文件命名规范（空格→下划线，大写→小写）
- 转换为绝对路径格式（以 `/` 开头）
- 未发布的文档链接会转换为纯文本（移除链接格式）

**实际转换示例：**
```markdown
<!-- Obsidian 中的相对路径链接 -->
[Book 1 Overview](Render%20Hell/Book%201%20Overview.md)
[Book 2 Pipeline](Render%20Hell/Book%202%20Pipeline.md)
[我的技术笔记](技术文档/我的技术笔记.md)

<!-- Hexo 格式（转换为绝对路径）-->
[Book 1 Overview](/book_1_overview)
[Book 2 Pipeline](/book_2_pipeline)
[我的技术笔记](/wo_de_ji_shu_bi_ji)
```

## 同文档内锚点链接转换

对于指向当前文档内标题的锚点链接，转换会应用以下规则：

- 以 `#` 开头的链接被识别为当前文档内锚点
- 自动生成当前文档的绝对路径前缀
- 锚点部分应用相同的转换规则（空格和点号→下划线）


**实际转换示例：**
```markdown
[跳转到结论](#结论部分)
[See Section](#Important Notes)

<!-- Hexo 格式 -->
[跳转到结论](/current_document/#结论部分)
[See Section](/current_document/#Important_Notes)
```

## 跨文档锚点链接转换

对于指向其他文档特定标题的链接，转换会应用如下规则：

- 文档路径转换为绝对路径格式
- 锚点部分：空格和点号转换为下划线 `_`（不是连字符）
- 保持原始大小写（不强制小写）
- 组合格式：`/文档名/#锚点名`

**实际转换示例：**
```markdown
<!-- Obsidian 格式 -->
[Book 1 Overview](Render%20Hell/Book%201%20Overview.md#Section Title)

<!-- Hexo 格式（实际转换结果）-->
[Book 1 Overview](/book_1_overview/#Section_Title)
```

## 资源文件链接转换

对于文档中引用的图片和其他资源文件，会保持原有的目录结构，但转换为对应的 Asset Folder 路径，具体转换规则如下：

- 资源文件转换到对应的 Asset Folder 中
- 路径格式为 `/article_name/converted_filename` 格式
- **自动移除 Obsidian 图片尺寸语法**（如 `|500`, `|800x600` 等）
- Hexo 不支持 Obsidian 的内联尺寸规格语法

**实际转换示例：**
```markdown
<!-- Obsidian 中的相对路径资源 -->
![Image](Render%20Hell/image.png)
![Document](Render%20Hell/document.pdf)
![Screenshot|500](Render%20Hell/screenshot.png)

<!-- Hexo 格式（保持目录结构）-->
![Image](/book_1_overview/image.png)
![Document](/book_1_overview/document.pdf)
![Screenshot](/book_1_overview/screenshot.png)
```