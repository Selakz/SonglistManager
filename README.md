一款使用 WPF 开发的用于可视化生成 Arcaea Songlist 信息和导出 ArcCreate 游玩包的小软件。如果你只能在 ArcCreate 游玩谱面，但是又不用 ArcCreate 写谱，相信这个工具可以帮助到你。

因为借鉴了 ArcCreate 的部分数据定义和导出逻辑，因此使用 GPL-3.0 License。涉及到的内容主要在 SonglistManager.ArcCreate 和 SonglistManager.Extensions 命名空间下，以及一些调用它们的函数。其他部分可以随意借鉴，无需在意 License（虽然没什么好借鉴的）

# Tips

- 树状文件管理器上方按钮左键用来选择根目录，而右键可以打开根目录。

- 可以读取谱面信息的对象包括：Arcade 工程文件夹（目前仅对 Arcade One 和 Arcade Plus 进行测试）、ArcCreate 工程文件夹、包含曲目信息的 songlist 文件。

- 当读取工程文件夹时，导出游玩包将试图读取该项目文件夹内的谱面、音频、曲绘等文件。

- 当读取 songlist 文件时，如果该 songlist 位于工程文件夹内，则会自动关联到这个项目；如果该 songlist 的同级目录中含有工程文件夹，将尝试以 songlist 中存储的曲名或 id 与对应的工程文件夹对应。

- 最佳实践：将所有谱面工程放在同一个目录中（当然也可以分级存储），若要导出单曲的 songlist，则导出到工程文件夹内；若要导出多曲的 songlist，则导出到该目录中。

- SonglistManager 会在自身同目录下生成一个 slst-settings.json，用于持久化部分设置信息。

# Credit

[Arcthesia - ArcCreate](https://github.com/Arcthesia/ArcCreate)

[iNKORE-NET - UI.WPF.Modern](https://github.com/iNKORE-NET/UI.WPF.Modern)
