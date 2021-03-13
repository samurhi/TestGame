using UnityEngine;

public class CharacterChanger : MonoBehaviour
{
    public PlayerMovement2 player;

    public void SkillGain(string skill)
    {
        switch (skill)
        {
            case "air jump":
                player.canAirJump = true;
                return;
            case "dash":
                player.canDash = true;
                return;
            case "ground pound":
                player.canGroundpound = true;
                return;
        }
    }
}
