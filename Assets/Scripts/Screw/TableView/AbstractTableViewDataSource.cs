using UnityEngine;

public abstract class AbstractTableViewDataSource : MonoBehaviour
{
    public abstract int GetNumberOfSections(TableView tableView);
    public abstract int GetNumberOfRows(TableView tableView, int section);

    public abstract float GetHeaderHeight(TableView tableView, int section);
    public abstract float GetRowHeight(TableView tableView, int section, int row);
    public abstract float GetRowSpacing(TableView tableView, int section);

    public abstract TableViewCell GetSectionHeader(TableView tableView, int section);
    public abstract TableViewCell GetCell(TableView tableView, int section, int row);
}