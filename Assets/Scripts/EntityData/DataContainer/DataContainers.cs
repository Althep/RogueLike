using UnityEngine;

public class BattleData : IDataContainer
{
    public LivingEntity myData;
    public LivingEntity targetData;
    public int value;

    public BattleData(LivingEntity myEntity)
    {
        myData = myEntity;
    }

    public void SetData(LivingEntity entity)
    {
        targetData = entity;
    }


    public void Reset()
    {
        targetData = null;
        value = 0;
    }

    public int Get()
    {
        return value;
    }
}

public class EntityContainer: IDataContainer
{
    public LivingEntity myData;
    public float value;

    public EntityContainer(LivingEntity entity)
    {
        myData = entity;
    }
    public void SetData(LivingEntity entity)
    {
        myData = entity;
    }

    public void Reset()
    {
        value = 0;
    }

    public float Get()
    {
        return value;
    }
}

public class ItemUserContainer : IDataContainer
{
    public LivingEntity myData;
    public ItemBase item;
    public float value;

    public ItemUserContainer(LivingEntity myData)
    {
        this.myData=myData;
    }

    public void SetData(ItemBase item)
    {
        this.item = item;
    }

    public void Reset()
    {
        item = null;
        value = 0;
    }

    public float Get()
    {
        return value;
    }
}

public class MagicContainer : IDataContainer
{
    public LivingEntity myData;
    public MagicBase magic;
    public float value;

    public MagicContainer(LivingEntity livingEntity)
    {
        myData = livingEntity;
    }

    public void SetData(MagicBase magic)
    {
        this.magic = magic;
    }
    public float Get()
    {
        return value;
    }
}
