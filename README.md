# Gyro
Gyro is a dependency injection (sort of) library for unity,
based on [Axis](https://github.com/sleitnick/Axis)

## Example
```csharp
public class LegumeStartExtension : IExtension {
    public static void OnPrepare(Type provider) {
        var legumeStartField = provider.GetField("_legumeStartAmount", BindingFlags.NonPublic | BindingFlags.Static);
        if (legumeStartField == null) return;
        // Passing null because it is static
        legumeStartField.SetValue(null, 10);
    }
}

[UseExtension(typeof(LegumeStartExtension))]
public class BeanProvider : IProvider {
    private static int _legumeStartAmount;
    public static void Start() {
        Debug.Log("Starting with " + _legumeStartAmount + " beans.");
    }
}

// Somewhere else
Builder.AddProvider(typeof(BeanProvider));
// Alternatively add the extension like this:
// Builder.AddExtension(typeof(LegumeStartExtension))
Builder.Start();
```