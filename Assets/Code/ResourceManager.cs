using UnityEngine;
using TMPro;
public class ResourceManager : MonoBehaviour
{
    [SerializeField] private int woodAmount = 0;
    [SerializeField] private int stoneAmount = 0;
    [SerializeField] private int ironAmount = 0;
    [SerializeField] private int goldAmount = 0;
    [SerializeField] private TextMeshProUGUI woodAmountText;
    [SerializeField] private TextMeshProUGUI stoneAmountText;
    [SerializeField] private TextMeshProUGUI ironAmountText;
    [SerializeField] private TextMeshProUGUI goldAmountText;

    public int GetWoodAmount()
    {
        return woodAmount;
    }
    public int GetStoneAmount()
    {
        return stoneAmount;
    }
    public int GetIronAmount()
    {
        return ironAmount;
    }
    public int GetGoldAmount()
    {
        return goldAmount;
    }
    public void SetWoodAmount(int amount)
    {
        woodAmount = amount;
        updateUI();
    }
    public void SetStoneAmount(int amount)
    {
        stoneAmount = amount;
        updateUI();
    }
    public void SetIronAmount(int amount)
    {
        ironAmount = amount;
        updateUI();
    }
    public void SetGoldAmount(int amount)
    {
        goldAmount = amount;
        updateUI();
    }
    void start()
    {
        updateUI();
    }

    void updateUI()
    {
        woodAmountText.text = woodAmount.ToString();
        stoneAmountText.text = stoneAmount.ToString();
        ironAmountText.text = ironAmount.ToString();
        goldAmountText.text = goldAmount.ToString();
    }

}
