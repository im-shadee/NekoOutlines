# NekoOutlines
> A lightweight, customizable outline toolkit for Unity 2D.

NekoOutlines provides customizable outline effects for Unity sprites and UI elements, with support for both inner and outer outlines.

The tool was designed with 2D workflows in mind, with a focus on clean, consistent outlines that work well with both simple and complex sprite shapes.


## 🧩 Features
- Customizable outer/inner outlines
- Pixel-perfect outline rendering for 2D sprites
- Anti-aliased outline support
- Support for Unity UI elements **and** `SpriteRenderer`
- Configurable outline color and thickness

---

This image shows the kind of results you can expect from NekoOutlines using both inner and outer outlines:

<img width="1596" height="666" alt="demo_diag_outlines" src="https://github.com/user-attachments/assets/5239dc69-ca5d-411b-95f5-c233d8876ffc" />


As mentionned just above, NekoOutlines is also compatible with `SpriteRenderers`. The picture below was taken from the `UI_Sprite_Test` scene within the "Samples" folder, showcasing outlines working on `SpriteRenderers`.

<img width="885" height="510" alt="demo_runtime_outlines" src="https://github.com/user-attachments/assets/f6a8e0fc-79c5-4c5d-a240-691f6cb840ac" />

NekoOutlines works directly with Unity's 2D rendering pipeline, allowing outlines to be applied to sprites without having to manually edit or create additional textures.


## 🎯 Who is this for?

NekoOutlines is intended for 2D-based Unity projects, especially workflows involving pixel art, sprites, and UI elements.

It can be useful for:
- Developers looking for an alternative to Unity's default outline effects, with more customization options
- Developers working with 2D sprites and UI


## 📦 Installation

> NekoOutlines is distributed as a Unity package.

To install NekoOutlines:

1. Open the Unity Editor and go to *Window > Package Management > Package Manager*
2. Click the "+" icon on the top left of the window. Click on "Install package from git URL"

<img width="277" height="228" alt="image" src="https://github.com/user-attachments/assets/b6353709-bb4c-44a6-bb28-8e34a5e3b5d1" />

3. Finally, enter this link: `https://github.com/im-shadee/NekoOutlines.git?path=/Package`, and press "Install". Done!

---

⚠️ This package also includes test assets and examples intended to show how to use NekoOutlines with your own sprites and UI elements.

To install them, go back to *Window > Package Management > Package Manager*, click on "NekoOutlines", and finally, go to the "Samples" tab. Click on "Import".

<img width="1494" height="288" alt="image" src="https://github.com/user-attachments/assets/67ced587-183d-4382-8d54-1494887a5b0e" />


## 📄 License

NekoOutlines uses two separate licenses: one for the source code and one for the visual assets included with the package.

The NekoOutlines source code is released under the MIT License. You are free to use, modify, distribute, and fork the source code, including in commercial projects. See LICENSE for the complete license text.

All sprites, textures, and visual demo assets located within the Samples folder of this repository are © im-shadee, All Rights Reserved. **These assets are provided solely to demonstrate the features of NekoOutlines and are not covered by the MIT License**.

**Terms of Use**

1. **Demo purposes only**: These visual assets are provided solely for the purpose of demonstrating the features of NekoOutlines within this package.
2. **No redistribution or reuse**: You are strictly prohibited from extracting, reusing, redistributing, selling, or modifying any of the sample artwork for use in other projects, commercial products, or public repositories.
3. **Separate licensing**: These restrictions apply only to the visual media and sample assets. The core source code of NekoOutlines is governed separately by the MIT License found in the root directory.

## 💭 Developer Note

This project started because I've always disliked the way Unity's default outline effects behave around rounded corners and complex shapes. I wanted something that gave me more control over how outlines are rendered, especially for pixel-art and 2D projects.

NekoOutlines is also my second public Unity tool, following [NekoPalettes](https://github.com/im-shadee/NekoPalettes). I hope it can be useful to other developers who want clean, customizable outlines without having to build their own shader from scratch.

Thanks for using NekoOutlines!
