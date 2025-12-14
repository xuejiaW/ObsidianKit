# Backup Command

`obk backup` 命令用于备份和恢复 Obsidian 仓库。备份会将仓库文件打包为 ZIP 格式，自动排除配置的忽略路径（如 `.git`、`.obsidian`、`.trash`）。

## 命令列表

### 创建备份
```bash
obk backup [--vault-dir <path>]
```

创建 Obsidian 仓库的备份，文件名自动使用 `<vault-name>_YYYYMMDD_HHMMSS.zip` 格式。

**参数：**
- `--vault-dir <path>`: 可选，指定 Obsidian 仓库路径。如果不指定，将使用配置中的路径。

**示例：**
```bash
# 使用配置的仓库路径
obk backup

# 指定仓库路径
obk backup --vault-dir D:\MyVault
```

### 列出所有备份
```bash
obk backup list
```

显示所有备份文件，包括序号、名称、大小和创建时间。

**输出示例：**
```
[1] PersonalNotes_20251214_180139    125 MB  2025-12-14 18:01:39
[2] PersonalNotes_20251213_100000    120 MB  2025-12-13 10:00:00
[3] PersonalNotes_20251212_150000    118 MB  2025-12-12 15:00:00

Total: 3 backup(s)
```

### 恢复备份
```bash
obk backup restore <backup-name> [--target-dir <path>]
```

恢复指定的备份到目标目录。

**参数：**
- `<backup-name>`: 备份名称（不含 `.zip` 扩展名）
- `--target-dir <path>`: 可选，指定恢复的目标目录。如果不指定，会在当前目录创建 `<vault-name>_restored` 文件夹。

**示例：**
```bash
# 恢复到默认位置
obk backup restore PersonalNotes_20251214_180139

# 恢复到指定目录
obk backup restore PersonalNotes_20251214_180139 --target-dir D:\RestoredVault
```

### 删除备份
```bash
obk backup remove <indices-or-name>
```

删除指定的备份文件，支持多种格式。

**参数格式：**
- **单个序号**：`1` - 删除第 1 个备份
- **多个序号**：`1,3,5` - 删除第 1、3、5 个备份
- **序号范围**：`1~3` - 删除第 1 到第 3 个备份
- **混合使用**：`1,3~5,7` - 删除第 1、3、4、5、7 个备份
- **备份名称**：`PersonalNotes_20251214_180139` - 使用完整备份名称

**示例：**
```bash
# 使用序号删除
obk backup remove 1

# 删除多个备份
obk backup remove 1,3

# 删除范围
obk backup remove 2~5

# 使用备份名称
obk backup remove PersonalNotes_20251214_180139
```

### 配置备份目录
```bash
obk backup config backup-dir <directory>
```

设置备份文件的存储目录。

**示例：**
```bash
obk backup config backup-dir D:\Backups
```

### 显示配置
```bash
obk backup config --list
```

显示当前的备份配置。

**输出示例：**
```
Backup Configuration:
====================
Backup Directory: D:\Backups
```

## 备份规则

### 自动排除路径

备份会自动排除 `config` 中配置的 `globalIgnoresPaths`，默认包括：
- `.git` - Git 版本控制文件
- `.obsidian` - Obsidian 配置文件
- `.trash` - 回收站文件

### 文件名格式

备份文件名自动使用以下格式：
```
<vault-name>_YYYYMMDD_HHMMSS.zip
```

例如：`PersonalNotes_20251214_180139.zip`

### 压缩级别

使用快速压缩模式（`CompressionLevel.Fastest`），在保证压缩效果的同时提供更快的备份速度。

## 使用场景

### 场景 1：定期备份

```bash
# 配置备份目录
obk backup config backup-dir D:\ObsidianBackups

# 创建备份
obk backup

# 查看所有备份
obk backup list
```

### 场景 2：清理旧备份

```bash
# 查看所有备份
obk backup list

# 输出：
# [1] Notes_20251214_180139    125 MB  2025-12-14 18:01:39
# [2] Notes_20251213_100000    120 MB  2025-12-13 10:00:00
# [3] Notes_20251212_150000    118 MB  2025-12-12 15:00:00
# [4] Notes_20251211_120000    115 MB  2025-12-11 12:00:00
# [5] Notes_20251210_100000    110 MB  2025-12-10 10:00:00

# 删除最旧的两个备份
obk backup remove 4~5

# 或删除多个不连续的备份
obk backup remove 2,4,5
```

### 场景 3：迁移或恢复

```bash
# 在旧电脑上创建备份
obk backup --vault-dir D:\OldVault

# 将备份文件复制到新电脑

# 在新电脑上恢复
obk backup restore OldVault_20251214_180139 --target-dir D:\NewVault
```

### 场景 4：测试前备份

```bash
# 安装新插件或进行大改动前
obk backup

# 如果出现问题，恢复备份
obk backup restore PersonalNotes_20251214_180139
```

## 注意事项

- **删除操作不可逆**：删除备份文件后无法恢复，请谨慎操作
- **恢复会覆盖**：恢复备份到已存在的目录时，会覆盖同名文件
- **空间占用**：备份文件会占用磁盘空间，建议定期清理旧备份
- **配置不备份**：默认不备份 `.obsidian` 配置文件夹，如需备份配置请修改 `globalIgnoresPaths`
- **进度显示**：备份大型仓库时会显示进度条，可以实时查看备份进度

## 相关命令

- `obk config obsidian-vault-dir` - 配置默认的 Obsidian 仓库路径
- `obk config ignore` - 管理全局忽略路径
- `obk template` - 管理 Obsidian 配置模板
