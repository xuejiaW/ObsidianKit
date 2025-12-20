# Bloat Command

`obk doctor bloat` 命令用于检测 Obsidian 仓库中过大的资源文件，帮助识别可能需要优化或清理的文件。

## 基本用法

```bash
obk doctor bloat [--vault-dir <path>]
```

## 参数

- `--vault-dir <path>`: 可选，指定 Obsidian 仓库路径。如果不指定，将使用配置中的路径。

## 工作原理

1. **扫描所有文件**：遍历仓库中的所有文件
2. **检查大小限制**：根据文件扩展名和配置的大小限制进行比较
3. **分页显示结果**：以表格形式展示超过阈值的文件
4. **提供统计信息**：显示文件总数和超出总量

## 默认大小限制

| 文件类型 | 扩展名 | 默认限制 |
|---------|--------|---------|
| 图片文件 | `.png`, `.jpg`, `.jpeg`, `.gif`, `.webp` | 1 MB |
| 视频文件 | `.mp4`, `.mov`, `.avi` | 10 MB |
| 音频文件 | `.mp3`, `.wav`, `.m4a` | 5 MB |
| 文档文件 | `.pdf`, `.docx` | 2 MB |
| Markdown | `.md` | 500 KB |
| 其他文件 | - | 2 MB（默认） |

## 使用示例

### 基本检查
```bash
obk doctor bloat
```

### 指定仓库路径
```bash
obk doctor bloat --vault-dir D:\MyVault
```

## 输出示例

```
Scanning Obsidian vault: D:\MyVault
Checking for oversized files...
====================================

Found 12 oversized file(s):

File                                                          Size          Limit        Exceeded
------------------------------------------------------------------------------------------------------------
assets/demo.gif                                               8.5 MB        1 MB         750.0%
images/tutorial-video.mp4                                     15.2 MB       10 MB        52.0%
assets/screenshots/fullscreen.png                             2.3 MB        1 MB         130.0%
documents/presentation.pdf                                    3.8 MB        2 MB         90.0%
...

-- More (10/12) -- Press Enter/Space for more, Q to quit: 

Total: 12 file(s), Total excess: 18.5 MB
```

## 配置管理

### 查看当前配置

```bash
obk doctor bloat config --list
```

输出示例：
```
    Doctor Bloat Configuration:
      Default Max Size: 2 MB
      Ignore Patterns (2):
        - **/*.tmp
        - Archives/**
      File Size Limits (13):
        .avi       : 10 MB
        .docx      : 2 MB
        .gif       : 1 MB
        .jpeg      : 1 MB
        .jpg       : 1 MB
        .m4a       : 5 MB
        .md        : 500 KB
        .mov       : 10 MB
        .mp3       : 5 MB
        .mp4       : 10 MB
        .pdf       : 2 MB
        .png       : 1 MB
        .wav       : 5 MB
        .webp      : 1 MB
```

### 管理忽略模式

#### 添加忽略模式
```bash
obk doctor bloat config ignore add "**/*.tmp"
obk doctor bloat config ignore add "Archives/**"
obk doctor bloat config ignore add "*.pdf"
```

支持的通配符：
- `*` - 匹配任意字符（不包括路径分隔符）
- `**` - 匹配任意字符（包括路径分隔符）
- `?` - 匹配单个字符

#### 移除忽略模式
```bash
obk doctor bloat config ignore remove "**/*.tmp"
```

#### 列出所有忽略模式
```bash
obk doctor bloat config ignore list
```

### 管理文件大小限制

#### 设置特定扩展名的限制
```bash
# 设置 GIF 限制为 5MB
obk doctor bloat config limit set .gif 5MB

# 设置 PNG 限制为 2MB
obk doctor bloat config limit set .png 2MB

# 设置视频限制为 20MB
obk doctor bloat config limit set .mp4 20MB
```

支持的大小格式：
- `500KB` - 千字节
- `2MB` - 兆字节
- `1.5MB` - 支持小数
- `1GB` - 千兆字节

#### 移除特定扩展名的限制
```bash
obk doctor bloat config limit remove .gif
```

移除后将使用默认大小限制。

#### 列出所有大小限制
```bash
obk doctor bloat config limit list
```

### 设置默认大小限制

```bash
# 设置默认为 5MB
obk doctor bloat config default-size 5MB
```

默认大小用于未配置特定限制的文件类型。

## 交互功能

检查结果会分页显示，支持以下操作：

- **Enter / Space / ↓** - 显示下一页
- **Q / Esc** - 退出查看

每页显示的行数会根据终端高度自动调整。

## 典型使用场景

### 场景 1：识别大文件
```bash
# 检查哪些文件过大
obk doctor bloat
```

### 场景 2：调整 GIF 限制
```bash
# 发现大量 GIF 超标，调整阈值
obk doctor bloat config limit set .gif 5MB

# 再次检查
obk doctor bloat
```

### 场景 3：忽略特定目录
```bash
# 忽略归档目录
obk doctor bloat config ignore add "Archives/**"

# 忽略所有 PDF
obk doctor bloat config ignore add "**/*.pdf"

# 检查（忽略后）
obk doctor bloat
```

### 场景 4：配合 clean 命令清理
```bash
# 第一步：找出大文件
obk doctor bloat

# 第二步：清理未引用的图片
obk doctor clean

# 第三步：再次检查
obk doctor bloat
```

## 注意事项

- 📊 **只是检测**：此命令只显示超大文件，不会删除或修改任何文件
- 🔍 **全局忽略**：会自动忽略 `.git`、`.obsidian`、`.trash` 等配置的全局忽略路径
- 💡 **优化建议**：
  - 对于大的 GIF 文件，可以考虑转换为视频格式（MP4）
  - 对于大的图片，可以压缩或调整分辨率
  - 对于大的文档，可以移到外部存储并使用链接
- 🎯 **阈值设置**：根据你的实际需求调整阈值：
  - 个人笔记：可以较宽松（如 GIF 5-10MB）
  - 发布博客：应该较严格（如 GIF 1-2MB）
  - 团队协作：考虑网络和存储限制

## 配置文件位置

配置保存在 ObsidianKit 的配置文件中：
- Windows: `%APPDATA%\ObsidianKit\config.json`
- macOS/Linux: `~/.config/ObsidianKit/config.json`

配置示例：
```json
{
  "doctor-bloat": {
    "ignorePatterns": [
      "**/*.tmp",
      "Archives/**"
    ],
    "fileSizeLimits": {
      ".gif": 5242880,
      ".png": 2097152,
      ".mp4": 10485760
    },
    "defaultMaxSize": 2097152
  }
}
```
