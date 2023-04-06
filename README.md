# Barracuda Inference Toolkit

Barracuda Inference Toolkit is a custom Unity package for performing inference with the [Barracuda inference library](https://docs.unity3d.com/Packages/com.unity.barracuda@3.0/manual/index.html). This package includes a flexible `BarracudaModelRunner` base class and various subclasses, such as `MultiClassImageClassifier`, for different tasks.



## Features

- Perform inference using the Unity Barracuda library
- Extendable `BarracudaModelRunner` base class for custom inference tasks
- Pre-built subclasses, such as `MultiClassImageClassifier`, for various tasks
- Support for different compute backends and channel orders



## Getting Started

### Prerequisites

- Unity game engine
- Barracuda package installed in your Unity project

### Installation

You can install the Barracuda-Inference-Toolkit package using the Unity Package Manager:

1. Open your Unity project.
2. Go to Window > Package Manager.
3. Click the "+" button in the top left corner, and choose "Add package from git URL..."
4. Enter the GitHub repository URL: `https://github.com/cj-mills/unity-barracuda-inference-toolkit.git`
5. Click "Add". The package will be added to your project.

For Unity versions older than 2021.1, add the Git URL to the `manifest.json` file in your project's `Packages` folder as a dependency:

```json
{
  "dependencies": {
    "com.cj-mills.unity-barracuda-inference-toolkit": "https://github.com/cj-mills/unity-barracuda-inference-toolkit.git",
    // other dependencies...
  }
}
```



## Usage

Here's an example of using the `BarracudaInferenceToolkit.MultiClassImageClassifier` subclass:

```c#
using UnityEngine;
using CJM.BarracudaInferenceToolkit;

public class ExampleImageClassifier : MonoBehaviour
{
    public RenderTexture inputTexture;
    public MultiClassImageClassifier classifier;

    void Update()
    {
        // Execute the model on the provided input texture.
        float[] output = classifier.ExecuteModel(inputTexture);

        // Get the predicted class index.
        int predictedClassIndex = Array.IndexOf(output, output.Max());

        // Get the class name corresponding to the predicted class index.
        string predictedClassName = classifier.GetClassName(predictedClassIndex);

        // Print the predicted class name.
        Debug.Log($"Predicted class: {predictedClassName}");
    }
}
```



## License

This project is licensed under the MIT License. See the [LICENSE](Documentation~/LICENSE) file for details.
