# Hexo Butterfly 文件命名规范

在 Hexo Butterfly 主题中，无论用以作为博文的文件名和用以表示资源的图片名等都必须符合规范：

-   文章文件名直接用于生成 URL 路径
-   URL 必须符合 RFC 3986 标准
-   不支持中文字符的直接 URL 编码

## md 文件

对于 markdown 文件，我们使用 [`HexoUtils.ConvertPathForHexoPost`](xref:Obsidian2.Utilities.Hexo.HexoUtils.ConvertPathForHexoPost*) 将其转换为符合 Hexo URL 生成规范的文件名。

**示例转换：**

-   `"我的文章.md"` → `"wo_de_wen_zhang.md"`
-   `"Hello World.md"` → `"hello_world.md"`
-   `"API 2.0 设计.md"` → `"api_2_0_she_ji.md"`

## 资源文件名约定

对于图片资源资源文件不直接用于 URL 生成，我们采用保守策略：

-   仅转换空格为下划线（解决路径问题）
-   保留中文及其他的特殊字符

**实现方法：** [`HexoUtils.ConvertPathForHexoAsset`](xref:Obsidian2.Utilities.Hexo.HexoUtils.ConvertPathForHexoAsset*)

**示例转换：**

-   `"my image.png"` → `"my_image.png"`
-   `"设计图 v2.jpg"` → `"设计图_v2.jpg"`
-   `"API文档 (最新版).pdf"` → `"API文档_(最新版).pdf"`
-   `"流程图-2024.svg"` → `"流程图-2024.svg"`
