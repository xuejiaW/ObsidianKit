# Template Command

`obk template` 命令支持配置和应用 obsidian 仓库的模板，所谓模板指的是一个文件夹下关于 Obsidian 相关的配置（可以简单的理解为 .obsidian 文件夹和 Obsidian-Plugins 文件夹）。当将一个模板应用到一个 obsidian 仓库时，即会将该模板下的配置文件复制到目标仓库中。

`obk template —list` 用以列出所有的 template

将一个特定的 Obsidian 的文件夹添加为名为 name 的 template
`obk template create <name> [source-vault]`

将一个名为为 name 的 template 的 obsidian 配置，应用给另一个 obsidian 仓库。

`obk template apply <name> [target-folder]`