using Gyro.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gyro.Tests.Runtime
{
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
}