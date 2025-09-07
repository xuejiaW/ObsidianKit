# Hexo Butterfly 告示框格式规范

在 Hexo Butterfly 主题中，告示框（Admonition）用于突出显示重要信息、提示、警告等内容，其有以下的要求：

-   使用 `{% note %}` 标签语法创建告示框
-   支持预定义的样式类型：`info`、`warning`、`danger`、`success`
-   标签必须正确闭合：`{% note %}...{% endnote %}`

**语法格式：**

```markdown
{% note [type] %}
内容文本
{% endnote %}
```

我们通过函数 [`HexoUtils.ConvertToHexoAdmonition`](xref:Obsidian2.Utilities.Hexo.HexoUtils.ConvertToHexoAdmonition*) 将 Obsidian 中的告示框转换为符合 Butterfly 主题要求的格式：

系统支持两种主要的 Admonition 输入格式转换为 Butterfly 主题格式：

## MkDocs/Obsidian 风格转换

**输入格式**（MkDocs Callout 语法）：

```markdown
> [!note]
> This is a simple note

> [!warning]
> This is a warning message
> with multiple lines

> [!tip]
> This is a helpful tip
```

**输出格式**（Butterfly 主题）：

```markdown
{% note info %}
This is a simple note
{% endnote %}

{% note warning %}
This is a warning message
with multiple lines
{% endnote %}

{% note primary %}
This is a helpful tip
{% endnote %}
```

**类型映射关系**：

| MkDocs 类型 | Butterfly 类型            | 说明      |
| ----------- | ------------------------- | --------- |
| `note`      | `info`                    | 信息提示  |
| `tip`       | `primary`                 | 主要提示  |
| `warning`   | `warning`                 | 警告信息  |
| `caution`   | `warning`                 | 注意事项  |
| `fail`      | `danger`                  | 错误/危险 |
| `quote`     | `'fas fa-quote-left'`     | 引用块    |
| `cite`      | `'fas fa-quote-left'`     | 引用块    |
| `example`   | `'fas fa-list'`           | 示例      |
| `tldr`      | `'fas fa-clipboard-list'` | 摘要      |

**嵌套 Admonition 支持**：

```markdown
> [!note]
> This is the outer note
>
> > [!warning]
> > This is a nested warning
> > This continues the outer note
```

转换为：

```markdown
{% note info %}
This is the outer note
{% note warning %}
This is a nested warning
{% endnote %}
This continues the outer note
{% endnote %}
```

**实现方法：** [`AdmonitionStyleUtils.ConvertMkDocsToButterflyStyle`](xref:Obsidian2.Utilities.Admonition.AdmonitionStyleUtils.ConvertMkDocsToButterflyStyle*)

## CodeBlock 风格转换

**输入格式**（代码块语法）：

````markdown
```ad-note
This is a note content
with multiple lines
```
````

```ad-warning
This is a warning message
```

```ad-tip
This is a helpful tip
```

**输出格式**（Butterfly 主题）：

```markdown
{% note info %}
This is a note content
with multiple lines
{% endnote %}

{% note warning %}
This is a warning message
{% endnote %}

{% note primary %}
This is a helpful tip
{% endnote %}
```

**类型映射关系**：

| CodeBlock 类型 | Butterfly 类型        | 说明      |
| -------------- | --------------------- | --------- |
| `ad-note`      | `info`                | 信息提示  |
| `ad-tip`       | `primary`             | 主要提示  |
| `ad-warning`   | `warning`             | 警告信息  |
| `ad-quote`     | `'fas fa-quote-left'` | 引用块    |
| `ad-fail`      | `danger`              | 错误/危险 |

**实现方法：** [`AdmonitionStyleUtils.ConvertCodeBlockToButterflyStyle`](xref:Obsidian2.Utilities.Admonition.AdmonitionStyleUtils.ConvertCodeBlockToButterflyStyle*)
