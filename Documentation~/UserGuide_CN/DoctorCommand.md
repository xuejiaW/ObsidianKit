# Doctor Command

`obk doctor` 命令用于诊断和修复 Obsidian 仓库中的问题。

## conflict - 检测和解决冲突文件

```bash
obk doctor conflict [--vault-dir <path>]
```

检测并解决云同步工具（如 Dropbox、OneDrive）产生的冲突文件。

**参数：**
- `--vault-dir <path>`: 可选，指定 Obsidian 仓库路径。如果不指定，将使用配置中的路径。

**检测规则：**

扫描所有目录，查找格式为 `(设备名's conflicted copy YYYY-MM-DD)` 的文件，例如：
- `note (HomePC's conflicted copy 2025-12-14).md`
- `workspace (Surface's conflicted copy 2024-08-03).json`

**解决策略：**

冲突文件按原始文件名分组，命令会标记内容最多的文件（⭐ LARGEST），并提供交互式解决：

1. 如果最大文件是冲突文件且原始文件存在：删除原始文件，重命名冲突文件，删除其他冲突文件
2. 如果最大文件是冲突文件但原始文件不存在：重命名冲突文件，删除其他冲突文件
3. 如果最大文件是原始文件：删除所有冲突文件

**示例：**

```bash
# 使用配置的仓库路径
obk doctor conflict

# 指定仓库路径
obk doctor conflict --vault-dir D:\MyVault
```

**输出示例：**

```
Found 9 conflict files in 6 groups:

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Group: .obsidian\workspace.json
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ✓ workspace.json (Original)
    Lines: 296 | Characters: 9,796
  • workspace (HomePC's conflicted copy 2025-12-14).json ⭐ LARGEST
    Lines: 338 | Characters: 11,324

Do you want to automatically resolve conflicts? (y/N):
```

## 注意事项

- 删除和重命名操作不可撤销，建议操作前手动备份
- 通过字符数和行数判断内容量（字符数 > 行数）
- 可对每个冲突组单独选择是否处理
