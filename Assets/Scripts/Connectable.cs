using UnityEngine;
using System.Collections;

public abstract class Connectable : MonoBehaviour
{
    public enum ConnectionStates
    {
        Hidden,
        Preview,
        Built
    }

    public Transform connectionRef;

    [SerializeField] private int pointValue = 1;
    [SerializeField] private Texture2D connectIcon;

    private GameObject pipeModel;

    public ConnectionStates ConnectionState { get; protected set; }

    void OnGUI()
    {
        if (ConnectionState == ConnectionStates.Preview)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        
            GameManager.Instance.Hud.ShowWorldSpaceButton(connectIcon, screenPos, OnClick);
                
//            if (GUI.Button(GameManager.Instance.Hud.CenteredRect(new Rect(screenPos.x, screenPos.y, 80, 80)),
//                connectIcon, ""))
//            {
//                GameManager.Instance.Player.ScorePoints(pointValue, transform);
//                GameManager.Instance.GridBuilder.MakeConnection(this);
//            }
        }
    }

    public abstract void OnConnected();

    public virtual void Reset()
    {
        if (pipeModel)
        {
            Destroy(pipeModel.gameObject);
        }
    }

    public virtual void ShowPreview()
    {
        if (ConnectionState != ConnectionStates.Hidden)
            return;

        ConnectionState = ConnectionStates.Preview;
    }

    public void MakeConnection(Connectable other)
    {
        float dist = Vector3.Distance(connectionRef.position, other.connectionRef.position);

        pipeModel = Instantiate(GameManager.Instance.PipePrefab);

        pipeModel.transform.SetParent(transform);
        pipeModel.transform.localScale = new Vector3(1, 1, dist*0.5f);
        pipeModel.transform.position = connectionRef.position;
        pipeModel.transform.LookAt(other.connectionRef);

        OnConnected();
    }

    private void OnClick()
    {
        GameManager.Instance.Player.ScorePoints(pointValue, transform);
        GameManager.Instance.GridBuilder.MakeConnection(this);
    }
}
