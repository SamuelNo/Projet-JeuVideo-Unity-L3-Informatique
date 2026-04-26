using UnityEngine;
using UnityEngine.UI;

public class StatBarHandler : MonoBehaviour 
{
    public Image healthFill; 
    public Image manaFill;   

    public Transform target;
    public Vector3 offset = new Vector3(0, 3f, 0); 

    void Update()
    {
        if (target != null)
        {
            
            transform.position = Camera.main.WorldToScreenPoint(target.position + offset);
        }
    }

  
    public void SetValues(float currentHP, float maxHP, float currentMP, float maxMP)
    {
        if (healthFill != null && maxHP > 0) 
            healthFill.fillAmount = (float)currentHP / maxHP;
            
        if (manaFill != null && maxMP > 0) 
            manaFill.fillAmount = (float)currentMP / maxMP;
    }
    public void destroyStatBar() {
        Destroy(gameObject,1f);
    }
}