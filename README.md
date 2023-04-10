# Barracuda Inference Base

Barracuda Inference Base is a custom Unity package that provides a foundation for performing inference with the [Barracuda inference library](https://docs.unity3d.com/Packages/com.unity.barracuda@3.0/manual/index.html). It includes a flexible base class to extend with task-specific packages.



## Features

- Perform inference using the Unity Barracuda library
- Extensible `BarracudaModelRunner` base class for custom inference tasks
- Support for different compute backends and channel orders



## Extensions

* [unity-barracuda-inference-image-classification](https://github.com/cj-mills/unity-barracuda-inference-image-classification): Perform image classification using computer vision models.
* [unity-barracuda-inference-yolox](https://github.com/cj-mills/unity-barracuda-inference-yolox): Perform object detection using YOLOX models



## Getting Started

### Prerequisites

- Unity game engine

### Installation

You can install the Barracuda Inference Base package using the Unity Package Manager:

1. Open your Unity project.
2. Go to Window > Package Manager.
3. Click the "+" button in the top left corner, and choose "Add package from git URL..."
4. Enter the GitHub repository URL: `https://github.com/cj-mills/unity-barracuda-model-runner.git`
5. Click "Add". The package will be added to your project.

For Unity versions older than 2021.1, add the Git URL to the `manifest.json` file in your project's `Packages` folder as a dependency:

```json
{
  "dependencies": {
    "com.cj-mills.barracuda-model-runner": "https://github.com/cj-mills/unity-barracuda-model-runner.git",
    // other dependencies...
  }
}
```



## License

This project is licensed under the MIT License. See the [LICENSE](Documentation~/LICENSE) file for details.
