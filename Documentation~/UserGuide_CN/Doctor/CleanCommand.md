# Clean Command

`obk doctor clean` 命令用于查找并清理 Obsidian 仓库中未被任何笔记引用的图片文件。

## 基本用法

```bash
obk doctor clean [--vault-dir <path>]
```

## 参数

- `--vault-dir <path>`: 可选，指定 Obsidian 仓库路径。如果不指定，将使用配置中的路径。

## 工作原理

1. **扫描所有 Markdown 文件**：查找所有 `.md` 文件中的图片引用
2. **扫描所有图片文件**：查找仓库中的所有图片文件（PNG, JPG, JPEG, GIF, WebP 等）
3. **识别未引用图片**：找出没有被任何 Markdown 文件引用的图片
4. **移动到 .trash**：将未引用的图片移动到 `.trash` 文件夹（而非直接删除）

## 支持的图片格式

- `.png`
- `.jpg` / `.jpeg`
- `.gif`
- `.webp`
- `.svg`
- `.bmp`

## 使用示例

### 使用配置的仓库路径
```bash
obk doctor clean
```

### 指定仓库路径
```bash
obk doctor clean --vault-dir D:\MyVault
```

## 输出示例

```
Scanning vault: D:\MyVault
This may take a while for large vaults...

Found 15 unreferenced image(s):
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[1]  assets/old-screenshot.png                              2.3 MB
[2]  assets/temp-diagram.png                                1.5 MB
[3]  images/unused-photo.jpg                                3.2 MB
[4]  assets/test.gif                                        5.1 MB
[5]  assets/backup/image001.png                             800 KB
...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Total size: 45.2 MB

Move these files to .trash folder? (y/N): y

Moved 15 file(s) to .trash, freed 45.2 MB
Current .trash size: 128.7 MB
```

## 安全特性

- ✅ **不直接删除**：文件移动到 `.trash` 文件夹，可以随时恢复
- ✅ **防止覆盖**：如果目标文件已存在，自动添加时间戳后缀
- ✅ **手动确认**：显示完整列表后需要用户确认才执行
- ✅ **尊重全局忽略**：自动忽略配置中设置的路径（如 `.git`、`.obsidian` 等）

## 注意事项

- 🔍 **检查引用方式**：命令会查找以下引用格式：
  - Obsidian 格式：`![[image.png]]`
  - Markdown 格式：`![alt](image.png)`
  - HTML 格式：`<img src="image.png">`
  
- ⚠️ **嵌入在代码块中的图片引用**可能不会被识别

- 💡 **建议操作流程**：
  1. 运行 `obk doctor clean` 查看未引用图片列表
  2. 确认列表中的文件确实不需要
  3. 输入 `y` 移动到 `.trash`
  4. 验证一段时间后，可以手动清空 `.trash` 文件夹

- 📂 **恢复文件**：如果误删，可以从 `.trash` 文件夹中恢复：
  ```bash
  # 手动从 .trash 恢复
  move .trash\image.png assets\image.png
  ```

## 典型使用场景

- 📝 **清理旧笔记**：删除笔记后，相关图片可能残留
- 📸 **整理截图**：测试用的临时截图累积过多
- 🔄 **重构笔记**：重新组织笔记后，旧图片未删除
- 💾 **释放空间**：定期清理未使用的大文件（配合 `bloat` 命令使用）
