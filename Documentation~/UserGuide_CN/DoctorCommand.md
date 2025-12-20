# Doctor Command

`obk doctor` 命令用于诊断和修复 Obsidian 仓库中的问题。

## 子命令列表

Doctor 命令提供以下子命令，用于诊断和修复不同类型的问题：

### [conflict](Doctor/ConflictCommand.md) - 检测和解决冲突文件

检测并解决云同步工具（如 Dropbox、OneDrive）产生的冲突文件。

```bash
obk doctor conflict [--vault-dir <path>]
```

**主要功能：**
- 扫描所有目录查找冲突文件
- 按文件分组显示冲突
- 智能识别最新/最大的文件
- 提供交互式解决方案

[查看详细文档 →](Doctor/ConflictCommand.md)

---

### [clean](Doctor/CleanCommand.md) - 清理未引用的图片

查找并清理 Obsidian 仓库中未被任何笔记引用的图片文件。

```bash
obk doctor clean [--vault-dir <path>]
```

**主要功能：**
- 扫描所有 Markdown 文件中的图片引用
- 识别未被引用的图片
- 安全地移动到 `.trash` 文件夹
- 显示释放的空间大小

[查看详细文档 →](Doctor/CleanCommand.md)

---

### [bloat](Doctor/BloatCommand.md) - 检测过大的资源文件

检测 Obsidian 仓库中过大的资源文件，帮助识别需要优化的内容。

```bash
obk doctor bloat [--vault-dir <path>]
```

**主要功能：**
- 按文件类型检查大小限制
- 表格形式显示超标文件
- 分页显示大量结果
- 支持自定义大小阈值和忽略模式

[查看详细文档 →](Doctor/BloatCommand.md)

---

## 快速开始

```bash
# 检查并修复冲突文件
obk doctor conflict

# 清理未引用的图片
obk doctor clean

# 检测过大的文件
obk doctor bloat

# 指定仓库路径
obk doctor conflict --vault-dir D:\MyVault
obk doctor clean --vault-dir D:\MyVault
obk doctor bloat --vault-dir D:\MyVault
```

## 配置仓库路径

如果不想每次都指定 `--vault-dir`，可以配置默认路径：

```bash
obk config vault-dir D:\MyVault
```

配置后，所有 doctor 命令都会使用该路径。

## 推荐工作流

### 定期维护
```bash
# 每周运行一次，保持仓库整洁
obk doctor conflict  # 解决同步冲突
obk doctor bloat     # 检查大文件
obk doctor clean     # 清理无用图片
```

### 发布前检查
```bash
# 发布博客前的清理
obk doctor bloat     # 确保没有过大的文件
obk doctor clean     # 移除未使用的资源
```

### 迁移/整理时
```bash
# 重构笔记结构后
obk doctor conflict  # 解决可能的冲突
obk doctor clean     # 清理旧的图片引用
```

## 安全特性

所有 doctor 命令都设计了安全机制：

- ✅ **交互式确认**：重要操作前需要用户确认
- ✅ **不直接删除**：文件移动到 `.trash` 而非永久删除
- ✅ **详细信息**：显示完整的文件列表和大小
- ✅ **可恢复**：从 `.trash` 可以随时恢复文件

## 注意事项

- 建议在运行 doctor 命令前备份重要数据
- 查看输出信息，确认操作符合预期
- 定期清空 `.trash` 文件夹释放空间
- 可以配合使用多个子命令，达到最佳效果

