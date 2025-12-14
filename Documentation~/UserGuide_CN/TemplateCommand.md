# Template Command

`obk template` 命令用于管理和应用 Obsidian 仓库的配置模板。模板保存了 Obsidian 仓库的配置（`.obsidian` 文件夹和 `Obsidian-Plugins` 文件夹），可以快速应用到其他仓库。

## 命令列表

### 列出所有模板
```bash
obk template list
```
显示所有已配置的模板及其路径状态。

### 创建模板
```bash
obk template create <name> <source-vault>
```
- `<name>`: 模板名称
- `<source-vault>`: 源 Obsidian 仓库路径（必须包含 `.obsidian` 文件夹）

**示例：**
```bash
obk template create my-config D:\ObsidianVaults\MyVault
```

### 应用模板
```bash
obk template apply <name> <target-folder>
```
- `<name>`: 要应用的模板名称
- `<target-folder>`: 目标文件夹路径

将模板的配置复制到目标文件夹。如果目标文件夹已有配置，会自动备份为 `.obsidian.backup.时间戳` 格式。

**示例：**
```bash
obk template apply my-config D:\NewVault
```

### 删除模板
```bash
obk template remove <name>
```
从配置中删除指定模板（不会删除源仓库文件）。

**示例：**
```bash
obk template remove old-config
```

## 注意事项

- 创建模板时只保存源路径引用，不复制文件
- 应用模板会完全替换目标的 `.obsidian` 和 `Obsidian-Plugins` 文件夹
- 应用前会自动备份目标文件夹的现有配置