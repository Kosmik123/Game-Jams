using UnityEngine;
using Bipolar;

public interface IMyInterface
{
	void MyMethod();
}

public class MyBehaviour : MonoBehaviour
{
	[SerializeField, RequireInterface(typeof(IMyInterface))]
	private Object mySerializedInterface;

	private void CallMyMethod()
	{
		((IMyInterface)mySerializedInterface).MyMethod();
	}
}
