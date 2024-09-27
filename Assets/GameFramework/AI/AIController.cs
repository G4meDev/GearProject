using UnityEngine;

public class AIController : MonoBehaviour
{
    public Vehicle vehicle;

    public AI_Route_Node aiRouteNode_Current;
    public AI_Route_Node aiRouteNode_Target;

    public void OnEnterNewRouteNode(AI_Route_Node node)
    {
        aiRouteNode_Current = node;

        UpdateTargetNode();
    }

    public void UpdateTargetNode()
    {
        if (aiRouteNode_Current.children.Count > 0)
        {
            aiRouteNode_Target = aiRouteNode_Current.children[0];
        }

        else
        {
            aiRouteNode_Current = null;
        }
    }

    void Start()
    {
        vehicle = GetComponent<Vehicle>();
    }

    void Update()
    {
        if (vehicle)
        {
            vehicle.vInput = 1;
        }

        if(aiRouteNode_Current && aiRouteNode_Target)
        {
            DrawHelpers.DrawSphere(aiRouteNode_Target.transform.position, 5, Color.blue);
        }
    }


}