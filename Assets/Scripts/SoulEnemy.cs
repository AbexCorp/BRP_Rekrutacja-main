using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SoulEnemy : MonoBehaviour, IEnemy
{
    [SerializeField] private GameObject InteractionPanelObject;
    [SerializeField] private GameObject ActionsPanelObject;
    [SerializeField] private SpriteRenderer EnemySpriteRenderer;

    private SpawnPoint _enemyPosition;
    public bool VulnerableToBow { get; private set; }
    public bool VulnerableToSword { get; private set; }
    public bool DiedToVulnerability { get; private set; }

    public void SetupEnemy(Sprite sprite, SpawnPoint spawnPoint)
    {
        EnemySpriteRenderer.sprite = sprite;
        _enemyPosition = spawnPoint;
        gameObject.SetActive(true);
    }

    public SpawnPoint GetEnemyPosition()
    {
        return _enemyPosition;
    }

    public GameObject GetEnemyObject()
    {
        return this.gameObject;
    }

    private void ActiveCombatWithEnemy()
    {
        GenerateVulnerability();
        ActiveInteractionPanel(false);
        ActiveActionPanel(true);
        GUIController.Instance.ChangeUISelection(ActionsPanelObject.GetComponentsInChildren<Selectable>().Where(x => x.interactable).FirstOrDefault().gameObject);
    }
    private void GenerateVulnerability()
    {
        float v = Random.Range(0f, 1f);
        if (v < 0.2)
            return;
        else if (v < 0.6f)
        {
            VulnerableToBow = true;
            VulnerabilityColor("Bow_Button");
        }
        else
        {
            VulnerableToSword = true;
            VulnerabilityColor("Sword_Button");
        }
    }
    private void VulnerabilityColor(string name)
    {
        var b = ActionsPanelObject.GetComponentsInChildren<Button>().Where(x => x.gameObject.name == name).FirstOrDefault();
        if(b != null)
        {
            ColorBlock c = b.colors;
            c.selectedColor = new Color(0.98f, 0.49f, 0.125f, 1);
            c.highlightedColor = c.selectedColor;
            b.colors = c;
        }
    }

    private void ActiveInteractionPanel(bool active)
    {
        InteractionPanelObject.SetActive(active);
    }

    private void ActiveActionPanel(bool active)
    {
        ActionsPanelObject.SetActive(active);
    }

    private void UseBow()
    {
        // USE BOW
        if (VulnerableToBow)
            DiedToVulnerability = true;
        GameEvents.EnemyKilled?.Invoke(this);
    }

    private void UseSword()
    {
        if (VulnerableToSword)
            DiedToVulnerability = true;
        GameEvents.EnemyKilled?.Invoke(this);
        // USE SWORD
    }

    #region OnClicks

    public void Combat_OnClick()
    {
        ActiveCombatWithEnemy();
    }

    public void Bow_OnClick()
    {
        UseBow();
    }

    public void Sword_OnClick()
    {
        UseSword();
    }

    #endregion
}


public interface IEnemy
{
    SpawnPoint GetEnemyPosition();
    GameObject GetEnemyObject();
}
