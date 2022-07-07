# Gyro
Gyro is a dependency injection (sort of) library for unity,
based on [Axis](https://github.com/sleitnick/Axis)

## Example
```csharp
public class LegumeStartExtension : IExtension {
    public static void OnPrepare(object instance, Type provider)
    {
        var legumeStartField = provider.GetField("_legumeStartAmount", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (legumeStartField == null) return;
        legumeStartField.SetValue(instance, 10);
    }
}

[UseExtension(typeof(LegumeStartExtension))]
public class BeanProvider : IProvider {
    private int _legumeStartAmount;

    public void Prepare() { }

    public void Start() {
        Debug.Log("Starting with " + _legumeStartAmount + " beans.");
    }
}

// Somewhere else
Builder.AddProvider(typeof(BeanProvider));
// Alternatively add the extension like this:
// Builder.AddExtension(typeof(LegumeStartExtension))
Builder.Start();
```