using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindUI : MonoBehaviour
{
    public WindTeller windTellPrefab;
    public RectTransform middle;
    public RectTransform canvas;

    public float tellerDistance = 3f;
    public float tellerScale = 0.3f;


    WindTeller teller;

    // Start is called before the first frame update
    void Start()
    {
        teller = Instantiate(windTellPrefab.gameObject).GetComponent<WindTeller>();
        teller.transform.localScale = Vector3.one * tellerScale;
        teller.correctPosition = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 pos = middle.position - canvas.transform.position;
        pos.x /= canvas.sizeDelta.x;
        pos.y /= canvas.sizeDelta.y;

        pos.x /= canvas.localScale.x;
        pos.y /= canvas.localScale.y;
        pos.z /= canvas.localScale.z;

        pos.x += 0.5f;
        pos.y += 0.5f;

        Ray r = Camera.main.ViewportPointToRay(pos);

        teller.transform.position = r.origin + r.direction * tellerDistance;
        teller.transform.localScale = Vector3.one * tellerScale;
        teller.line.widthMultiplier = tellerScale;
    }

    private void OnEnable()
    {
        if (teller) teller.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (teller) teller.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (teller) Destroy(teller.gameObject);
    }
}
