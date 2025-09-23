namespace Home.WebUI.Infrastructure.ChangeTrackers;

public struct PropertyChangeTracker<TProperty>
{

    #region Fields

    private bool m_HasBeenSet;
    private readonly TProperty m_Value;

    #endregion Fields

    #region Constructors

    public PropertyChangeTracker()
    {
        this.m_Value = default;
        this.m_HasBeenSet = false;
    }

    public PropertyChangeTracker(TProperty property)
    {
        this.m_Value = property;
        this.m_HasBeenSet = true;
    }

    #endregion Constructors

    #region Properties

    public bool HasBeenSet => this.m_HasBeenSet;

    public TProperty Value
    {
        get => this.m_Value;
        set => this = new PropertyChangeTracker<TProperty>(value);
    }

    #endregion Properties

    #region Methods

    public static implicit operator TProperty(PropertyChangeTracker<TProperty> property)
        => property.Value;

    #endregion Methods

}
