using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
	public WeaponController currentWeapon;
	public InventoryManager inventoryManager;
	public GameObject bulletIcon;
	public GameObject bulletUI;
	public RectTransform bulletDrawer;
	bool isReloading = false;
	Weapon currentlyEquippedItem;
	public Image equippedWeaponUI;
	Image[] bulletUIChildren;

	int clipCapacity;

	private void Start()
	{
		bulletUIChildren = bulletUI.transform.GetComponentsInChildren<Image>();
	}

	public void UpdateBulletUI(int bulletsInClip)
	{
		if (bulletUIChildren != null)
		{
			for (int i = 0; i < bulletUIChildren.Length; i++)
			{
				if (bulletUIChildren[i].transform.gameObject.activeSelf)
				{
					bulletUIChildren[i].transform.gameObject.SetActive(false);
					break;
				}
			}
		}
	}

	void SetBulletUICount()
	{
		if (currentWeapon.currentBulletsInClip < 7)
		{
			bulletDrawer.pivot = new Vector2(.81f, .5f);
		}
		else
		{
			bulletDrawer.pivot = new Vector2(.5f, .5f);
		}

		for (int i = 0; i < currentWeapon.currentBulletsInClip; i++)
		{
			GameObject bullet = Instantiate(bulletIcon);
			bullet.transform.SetParent(bulletUI.transform);
		}
		clipCapacity = currentWeapon.weapon.clipSize;
	}

	IEnumerator ReloadUI(Image[] childArray)
	{
		foreach (Image child in childArray)
		{
			yield return new WaitForSeconds(currentWeapon.weapon.reloadTime / clipCapacity);
			child.gameObject.SetActive(true);
		}
	}

	public void SetReloadingUI()
	{
		isReloading = true;
		StartCoroutine(ReloadUI(bulletUIChildren));
		isReloading = false;
	}

	public void SetWeapon(Weapon weapon)
	{
		equippedWeaponUI.sprite = weapon.itemIcon;
		equippedWeaponUI.SetNativeSize();
		currentlyEquippedItem = weapon;
		SetBulletUICount();
	}
}
