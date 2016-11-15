using UnityEngine;
using System.Collections;

public class EdgeDetectionShader : MonoBehaviour {
	public void showEdge(Vector3 _targetPosition, float _targetRotation){
		gameObject.transform.localPosition = new Vector3(_targetPosition.x, 1f, _targetPosition.z);
		Vector3 temp = gameObject.transform.localEulerAngles;
		gameObject.transform.localEulerAngles = new Vector3(temp.x, _targetRotation, temp.z);
	}

	public void hidEdge(Vector3 _targetPosition, float _targetRotation){
		gameObject.transform.localPosition = new Vector3(_targetPosition.x, -1f, _targetPosition.z);
		Vector3 temp = gameObject.transform.localEulerAngles;
		gameObject.transform.localEulerAngles = new Vector3(temp.x, _targetRotation, temp.z);
	}
}
