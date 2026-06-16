using UnityEngine;

public class UTS_GameController : MonoBehaviour
{
    [Header ("Doors")]
    private bool securityOpen;
    private bool corridorOpen;
    private bool officeOpen;
    private bool controlRoomOpen;
    private bool exitOpen;
    public float autoCloseDelay = 5f;
    private float openTimer = 0f;
    private Color accessColor = new Color(0f, 1f, 0f, 1f);
    private Color deniedColor = new Color(1f, 0f, 0f, 1f);

    private bool hasKeyCard;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hasKeyCard = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(corridorOpen || securityOpen || officeOpen || controlRoomOpen || exitOpen)
        {
            openTimer += Time.deltaTime;

            if(openTimer >= autoCloseDelay)
            {
                corridorOpen = false;
                securityOpen = false;
                officeOpen = false;
                controlRoomOpen = false;
                exitOpen = false;
                openTimer = 0f;
            }
        }
    }

    public void ToggleIsOpen(string door)
    {
        if(door == "corridor")
        {
            corridorOpen = !corridorOpen;
        }
        if(door == "security")
        {
            securityOpen = (hasKeyCard) ? !securityOpen : false;
        }
        if(door == "office")
        {
            officeOpen = !officeOpen;
        }
        if(door == "control")
        {
            controlRoomOpen = !controlRoomOpen;
        }
        if(door == "exit")
        {
            exitOpen = !exitOpen;
        }
    }

    public bool GetDoorStatus(string door)
    {
        if(door == "corridor")
        {
            return corridorOpen;
        }
        if(door == "security")
        {
            return securityOpen;
        }
        if(door == "office")
        {
            return officeOpen;
        }
        if(door == "control")
        {
            return controlRoomOpen;
        }
        if(door == "exit")
        {
            return exitOpen;
        }
        return false;
    }

    public Color GetDoorColor(string door)
    {
        if(door == "corridor")
        {
            return accessColor;
        }
        if(door == "security")
        {
            return deniedColor;
        }
        if(door == "office")
        {
            return accessColor;
        }
        if(door == "control")
        {
            return deniedColor;
        }
        if(door == "exit")
        {
            return deniedColor;
        }
        return deniedColor;
    }

    public void SetHasKeyCard()
    {
        hasKeyCard = true;
    }
}
