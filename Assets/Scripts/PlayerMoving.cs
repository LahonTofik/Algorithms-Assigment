using UnityEngine;
using UnityEngine.Events;

public class PlayerMoving : MonoBehaviour
{
    public UnityEvent<Vector3> OnClick;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hitInfo))
            {
                Vector3 clickWorldPosition = hitInfo.point;
                Debug.Log(clickWorldPosition);
                OnClick.Invoke(clickWorldPosition);
            }
        }
    }
}

