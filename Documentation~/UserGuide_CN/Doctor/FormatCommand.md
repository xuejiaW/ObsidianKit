# Format Command

`obk doctor format` 命令用于检查和修复 Obsidian 仓库中 Markdown 文件的格式问题，帮助保持文档的一致性和规范性。

## 基本用法

```bash
obk doctor format [--vault-dir <path>] [--fix] [--verbose] [--rules <rules>] [--config <file>]
```

## 参数

- `--vault-dir <path>`: 可选，指定 Obsidian 仓库路径。如果不指定，将使用配置中的路径。
- `--fix`: 可选，自动修复可修复的格式问题。
- `--verbose`: 可选，显示所有问题的详细列表。
- `--rules <rules>`: 可选，只检查/修复指定的规则（用逗号分隔）。
- `--config <file>`: 可选，使用指定的配置文件。

## 工作原理

1. **查找配置文件**：自动查找仓库中的 `.markdownlint-cli2.jsonc` 配置文件
2. **同步忽略模式**：将 ObsidianKit 的 ignore patterns 同步到 markdownlint 配置
3. **扫描 Markdown 文件**：遍历仓库中的所有 `.md` 文件
4. **检查格式规则**：根据配置的规则检查文件格式
5. **统计和报告**：生成详细的统计报告和问题列表
6. **自动修复**（可选）：修复可以自动修复的格式问题

## 使用示例

### 基本检查
```bash
obk doctor format
```

### 自动修复所有可修复的问题
```bash
obk doctor format --fix
```

### 查看详细问题列表
```bash
obk doctor format --verbose
```

### 只检查特定规则
```bash
# 只检查 MD030 规则
obk doctor format --rules MD030

# 检查多个规则
obk doctor format --rules MD030,MD031,MD047
```

### 只修复特定规则
```bash
# 只修复列表标记后的空格问题
obk doctor format --fix --rules MD030

# 修复多个规则
obk doctor format --fix --rules MD030,MD032,MD047
```

### 指定配置文件
```bash
obk doctor format --config .markdownlint-custom.jsonc
```

### 指定仓库路径
```bash
obk doctor format --vault-dir D:\MyVault
```

## 输出示例

### 基本检查输出
```
Using markdownlint-cli2 v0.20.0 (markdownlint v0.40.0)
Checking Markdown files in: C:\Users\user\Vault

Using config: .markdownlint-cli2.jsonc

Running format check...

Scanning files... Done!


Found 17 issue(s) in 7 file(s)

📊 Issues by Rule:

  MD030       8 ( 47.1%) ███████████████████████
           Spaces after list markers [Expected: 1; Actual: 3]
  MD049       2 ( 11.8%) █████
           Emphasis style [Expected: asterisk; Actual: underscore]
  MD056       2 ( 11.8%) █████
           Table column count [Expected: 3; Actual: 4]
  MD018       1 (  5.9%) ██
           No space after hash on atx style heading
  MD032       1 (  5.9%) ██
           Lists should be surrounded by blank lines

📁 Affected Files:

 10 issue(s): 00_Books/Book1.md
  2 issue(s): 10_Notes/Note1.md
  1 issue(s): 00_Books/Book2.md
  1 issue(s): 10_Notes/Note2.md
  1 issue(s): 20_Tutorials/Tutorial1.md

💡 Tips:
  • To fix issues automatically: obk doctor format --fix
  • To see details for a specific file: markdownlint-cli2 <file>
  • To learn about rules: https://github.com/DavidAnson/markdownlint/blob/main/doc/Rules.md
```

### 详细输出（--verbose）
```
📋 Detailed Issues List:

00_Books/Book1.md:42 [MD030] Spaces after list markers [Expected: 1; Actual: 3]
00_Books/Book1.md:43 [MD030] Spaces after list markers [Expected: 1; Actual: 3]
00_Books/Book1.md:120 [MD049] Emphasis style [Expected: asterisk; Actual: underscore]
10_Notes/Note1.md:15 [MD032] Lists should be surrounded by blank lines
...
```

### 修复完成输出
```
Using markdownlint-cli2 v0.20.0 (markdownlint v0.40.0)
Checking Markdown files in: C:\Users\user\Vault

Using config: .markdownlint-cli2.jsonc

Running with --fix (will modify files)...

markdownlint-cli2 v0.20.0 (markdownlint v0.40.0)
Finding: **/*.md
Linting: 1408 file(s)
Summary: 0 error(s)

✓ Format check and fix completed!
```

## 配置管理

### 查看当前配置

```bash
obk doctor format config --list
```

输出示例：
```
Format Configuration:
  Configuration file: .markdownlint-cli2.jsonc
  Ignore patterns (2):
    - _Draft/**
    - Templates/**
  
  Disabled rules:
    - MD013 (line-length)
    - MD033 (no-inline-html)
    - MD041 (first-line-heading)
```

### 管理忽略模式

#### 添加忽略模式
```bash
obk doctor format config ignore add "_Draft/**"
obk doctor format config ignore add "Templates/**"
obk doctor format config ignore add "Archive/**"
```

支持的通配符：
- `*` - 匹配任意字符（不包括路径分隔符）
- `**` - 匹配任意字符（包括路径分隔符）
- `?` - 匹配单个字符
- `!` - 排除模式（例如 `!*.md` 表示排除所有 Markdown 文件）

#### 移除忽略模式
```bash
obk doctor format config ignore remove "_Draft/**"
```

#### 列出所有忽略模式
```bash
obk doctor format config ignore list
```

输出示例：
```
Format ignore patterns (2):
  - _Draft/**
  - Templates/**
```

## 常见格式规则

### 推荐禁用的规则（适用于 Obsidian）

以下规则通常与 Obsidian 的使用习惯冲突，建议在 `.markdownlint-cli2.jsonc` 中禁用：

```jsonc
{
  "config": {
    // MD013: 行长度限制
    // 禁用原因：中文、长 URL、代码示例等经常超过限制
    "MD013": false,
    
    // MD028: blockquote 内不能有空行
    // 禁用原因：Obsidian callout 语法需要空行
    "MD028": false,
    
    // MD033: 禁止 HTML 标签
    // 禁用原因：Obsidian 支持 HTML
    "MD033": false,
    
    // MD034: 禁止裸 URL
    // 禁用原因：Obsidian 会自动渲染裸 URL
    "MD034": false,
    
    // MD041: 文件第一行必须是一级标题
    // 禁用原因：Obsidian 笔记可能以元数据开头
    "MD041": false,
    
    // MD045: 图片必须有 alt 文本
    // 禁用原因：笔记中图片经常不需要 alt
    "MD045": false,
    
    // MD051: 链接片段必须有效
    // 禁用原因：Obsidian 的标题 ID 规则与标准不同
    "MD051": false,
    
    // MD060: 表格列对齐
    // 禁用原因：无法自动修复，且对渲染无影响
    "MD060": false
  }
}
```

### 支持自动修复的规则

以下规则可以使用 `--fix` 参数自动修复：

- **MD004** - 无序列表样式统一
- **MD005** - 列表项缩进一致
- **MD007** - 无序列表缩进
- **MD009** - 移除行尾空格
- **MD010** - 将 Tab 替换为空格
- **MD011** - 修复反转的链接语法
- **MD012** - 删除多余的空行
- **MD018** - 在 ATX 标题的 # 后添加空格
- **MD019** - 移除 ATX 标题 # 后的多余空格
- **MD022** - 在标题前后添加空行
- **MD023** - 将标题移到行首
- **MD027** - 移除 blockquote 符号后的多余空格
- **MD030** - 列表标记后的空格数量
- **MD031** - 在代码块前后添加空行
- **MD032** - 在列表前后添加空行
- **MD034** - 为裸 URL 添加尖括号
- **MD037** - 移除强调标记内的空格
- **MD038** - 移除代码标记内的空格
- **MD039** - 移除链接文本内的空格
- **MD047** - 在文件末尾添加换行符
- **MD048** - 统一代码围栏风格
- **MD049** - 统一斜体风格
- **MD050** - 统一粗体风格

## 规则过滤工作原理

当使用 `--rules` 参数时：

1. 系统会创建一个临时配置文件（`.obk-temp.markdownlint-cli2.jsonc`）
2. 临时配置会：
   - 继承主配置的 `globs` 忽略模式
   - 禁用所有默认规则（`"default": false`）
   - 只启用指定的规则
3. 运行 markdownlint-cli2 时使用临时配置
4. 完成后自动清理临时配置文件

这样可以在不修改主配置文件的情况下，灵活地检查或修复特定规则。

## 最佳实践

### 1. 逐步修复

不要一次性修复所有问题，建议逐个规则修复：

```bash
# 第一步：修复空格问题
obk doctor format --fix --rules MD030

# 第二步：修复列表格式
obk doctor format --fix --rules MD032

# 第三步：修复文件末尾换行
obk doctor format --fix --rules MD047
```

### 2. 先检查后修复

在使用 `--fix` 之前，先检查会修复什么：

```bash
# 先查看问题
obk doctor format --verbose --rules MD030

# 确认后再修复
obk doctor format --fix --rules MD030
```

### 3. 版本控制

建议在版本控制系统中进行修复：

```bash
# 创建新分支
git checkout -b format-fixes

# 修复格式
obk doctor format --fix

# 检查变更
git diff

# 提交
git commit -m "Fix markdown formatting"
```

### 4. 配置文件管理

将配置文件纳入版本控制：

```bash
git add .markdownlint-cli2.jsonc
git commit -m "Add markdownlint configuration"
```

## 故障排查

### 问题：找不到配置文件

**症状**：
```
Warning: No configuration file found
```

**解决方案**：
1. 在仓库根目录创建 `.markdownlint-cli2.jsonc` 文件
2. 或使用 `--config` 参数指定配置文件

### 问题：ignore 模式不生效

**症状**：被忽略的文件仍然被检查

**解决方案**：
1. 检查 glob 模式是否正确
2. 使用 `obk doctor format config ignore list` 查看当前配置
3. 确保 ignore 模式在配置文件的 `globs` 数组中

### 问题：--fix 没有修复任何问题

**症状**：显示 "✓ Format check and fix completed!" 但文件未修改

**原因**：该规则不支持自动修复（如 MD060）

**解决方案**：
1. 查看规则文档确认是否支持自动修复
2. 考虑禁用无法自动修复的规则
3. 手动修复需要的格式问题

### 问题：路径显示错误

**症状**：文件路径显示为错误的绝对路径

**原因**：已修复，确保使用最新版本

**解决方案**：
1. 更新 ObsidianKit 到最新版本
2. 路径应该显示为相对于 vault 根目录的路径

## 相关资源

- [markdownlint 规则文档](https://github.com/DavidAnson/markdownlint/blob/main/doc/Rules.md)
- [markdownlint-cli2 文档](https://github.com/DavidAnson/markdownlint-cli2)
- [Obsidian Markdown 语法](https://help.obsidian.md/Editing+and+formatting/Basic+formatting+syntax)

## 另请参阅

- [Doctor Bloat Command](BloatCommand.md) - 检测过大的资源文件
- [Doctor Clean Command](CleanCommand.md) - 清理无效链接和孤立文件
- [Doctor Conflict Command](ConflictCommand.md) - 检测文件名冲突
- [Config Command](../Config.md) - 配置管理
