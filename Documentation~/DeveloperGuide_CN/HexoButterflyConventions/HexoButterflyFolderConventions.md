# Hexo Butterfly 目录结构约定

## Hexo Asset Folders 硬性规范

**Hexo 官方要求：** 当启用 Asset Folders 功能时，每篇文章的资源文件必须存放在与文章同名的目录中。

```
source/_posts/
├── my-post.md          # 文章文件
└── my-post/            # 资源目录（与文章同名）
    ├── image1.png      # 图片资源
    └── diagram.svg     # 其他资源
```

**Butterfly 主题要求：** 完全兼容 Hexo Asset Folders 规范，资源文件通过相对路径引用。

## Obsidian 到 Hexo 的目录映射

> [!note]
>
> Obsidian 本身并没有约定文件和资源的存放结构，但在实际使用中，我们 **假定** 采用集中式的 `assets/` 目录来管理所有资源文件。

**Obsidian 原始结构：**

```
vault/
├── MyArticle.md        # 笔记文件
└── assets/             # 全局资源目录
    └── MyArticle/      # 按笔记名称组织的资源
        ├── image1.png
        └── diagram.svg
```

**Hexo 目标结构：**

```
source/_posts/
├── my_article.md       # 转换后的文章文件
└── my_article/         # 转换后的资源目录
    ├── image1.png      # 资源文件（可能重命名）
    └── diagram.svg
```

**技术约定原因：** 
- Obsidian 使用集中式 `assets/` 目录管理，但 Hexo 要求分散式管理
- 需要将 `assets/ArticleName/` 映射为 `article_name/`
- 文件名需要符合 URL 安全标准

**实现方法：** [`HexoUtils.CreateHexoPostBundle`](xref:ObsidianKit.Utilities.Hexo.HexoUtils.CreateHexoPostBundle*) 方法实现完整的目录结构转换。
