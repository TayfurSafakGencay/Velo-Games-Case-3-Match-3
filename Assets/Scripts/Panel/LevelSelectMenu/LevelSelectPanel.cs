using Unity.Mathematics;
using UnityEngine;

namespace Panel.LevelSelectMenu
{
    public class LevelSelectPanel : MonoBehaviour
    {
        [SerializeField]
        private LevelSelectPanelItem _levelSelectPanelItem;

        [SerializeField]
        private Transform _itemContainer;

        public int LevelCount;

        private void Start()
        {
            for (int i = 1; i < LevelCount + 1; i++)
            {
                LevelSelectPanelItem item = Instantiate(_levelSelectPanelItem, transform.position, quaternion.identity, _itemContainer);
                item.Init(i, PlayerPrefs.GetInt("Level " + i, 0), false);
            }
        }
    }
}