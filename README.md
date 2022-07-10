# Gyro
Gyro is a dependency injection (sort of) library for unity,
based on [Axis](https://github.com/sleitnick/Axis)

## Example
```csharp
// LegumeStartExtension.cs
public class LegumeStartExtension : IExtension {
    public static void OnPrepare(object instance, Type provider)
    {
        var legumeStartField = provider.GetField("_legumeStartAmount", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (legumeStartField == null) return;
        legumeStartField.SetValue(instance, 10);
    }
}

// BeanProvider.cs
[UseExtension(typeof(LegumeStartExtension))]
public class BeanProvider : Provider {
    [SerializeField]
    private int multiplier = 2;

    private int _legumeStartAmount;
    public override void Prepare() { }
    public override void Begin()
    {
        Debug.Log("Starting with " + _legumeStartAmount * multiplier + " beans.");
    }
}

// Somewhere else
Builder.AddProvider(GetComponent<BeanProvider>());
// Alternatively add the extension like this:
// Builder.AddExtension(typeof(LegumeStartExtension))
Builder.Start();
```