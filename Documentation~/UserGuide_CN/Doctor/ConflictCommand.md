# Conflict Command

`obk doctor conflict` 命令用于检测和解决云同步工具（如 Dropbox、OneDrive）产生的冲突文件。

## 基本用法

```bash
obk doctor conflict [--vault-dir <path>]
```

## 参数

- `--vault-dir <path>`: 可选，指定 Obsidian 仓库路径。如果不指定，将使用配置中的路径。

## 检测规则

扫描所有目录（包括 `.obsidian`、`.trash` 等），查找格式为 `(设备名's conflicted copy YYYY-MM-DD)` 的文件，例如：
- `note (HomePC's conflicted copy 2025-12-14).md`
- `workspace (Surface's conflicted copy 2024-08-03).json`

## 解决策略

冲突文件按原始文件名分组，命令会标记内容最多的文件（⭐ LARGEST），并提供交互式解决：

1. **最大文件是冲突文件且原始文件存在**：删除原始文件，重命名冲突文件为原始文件名，删除其他冲突文件
2. **最大文件是冲突文件但原始文件不存在**：重命名冲突文件为原始文件名，删除其他冲突文件
3. **最大文件是原始文件**：删除所有冲突文件

判断标准：字符数优先，如果相同则比较行数。

## 使用示例

### 使用配置的仓库路径
```bash
obk doctor conflict
```

### 指定仓库路径
```bash
obk doctor conflict --vault-dir D:\MyVault
```

## 输出示例

```
Scanning Obsidian vault: D:\MyVault
Checking all directories (including .obsidian, .trash, etc.)
====================================

Found 9 conflict files in 6 groups:

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Group: .obsidian\workspace.json
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ✓ workspace.json (Original)
    Lines: 296 | Characters: 9,796
  • workspace (HomePC's conflicted copy 2025-12-14).json ⭐ LARGEST
    Lines: 338 | Characters: 11,324

Do you want to automatically resolve conflicts for this group? (y/N): y
✓ Resolved: Renamed conflict file to original, removed other conflicts

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Group: notes\meeting.md
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ✓ meeting.md (Original) ⭐ LARGEST
    Lines: 150 | Characters: 4,523
  • meeting (Surface's conflicted copy 2025-12-13).md
    Lines: 142 | Characters: 4,201

Do you want to automatically resolve conflicts for this group? (y/N): y
✓ Resolved: Removed all conflict files, kept original

Summary:
- Resolved: 2 groups
- Skipped: 4 groups
```

## 注意事项

- ⚠️ 删除和重命名操作不可撤销，建议操作前手动备份重要数据
- 内容量通过字符数和行数判断（字符数 > 行数）
- 可以对每个冲突组单独选择是否处理
- 处理后，建议在 Obsidian 中验证文件内容是否正确
