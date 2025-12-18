using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PlayingPanel : PanelBase
{
    private Transform LocalHp;
    private Image LocalHpImg;
    private TextMeshProUGUI LocalHpNum;

    private Transform RemoteHp;
    private Image RemoteHpImg;
    private TextMeshProUGUI RemoteHpNum;

    private Transform BulletsNum;
    private TextMeshProUGUI BulletsNumText;

    private void Awake()
    {
        LocalHp = transform.Find("MeMsg/Hp/Hp");
        LocalHpImg = LocalHp.GetComponent<Image>();
        LocalHpNum = transform.Find("MeMsg/Num/Num").GetComponent<TextMeshProUGUI>();

        RemoteHp = transform.Find("EnemyMsg/Hp/Hp");
        RemoteHpImg = RemoteHp.GetComponent<Image>();
        RemoteHpNum = transform.Find("EnemyMsg/Num/Num").GetComponent<TextMeshProUGUI>();

        BulletsNum = transform.Find("BulletsNum");
        BulletsNumText = BulletsNum.GetComponent<TextMeshProUGUI>();
    }

    public void ChangeLocalHp(int count)
    {
        LocalHpNum.text = count.ToString();
        Debug.Log("[PlayingPanel] ChangeLocalHp To" +count);
    }
    public void ChangeRemoteHp(int count)
    {
        RemoteHpNum.text = count.ToString();
    }
    public void SetBulletsNum(int count)
    {
        BulletsNumText.text = count.ToString();
    }
}
