using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SampleTableViewDataSource : AbstractTableViewDataSource
{
    public SectionData[] data;

    public override TableViewCell GetCell(TableView tableView, int section, int row)
    {
        var cell = tableView.GetReusableCell(data[section].cell.reuseIdentifier);
        if (cell == null) {
            var go = Instantiate(data[section].cell.gameObject) as GameObject;
            cell = go.GetComponent<TableViewCell>();    
        }

        cell.gameObject.name = cell.GetComponentInChildren<Text>().text = string.Format("Content {0}:{1}", section, row);
        return cell;
    }

    public override float GetHeaderHeight(TableView tableView, int section)
    {
        if (data[section].header == null)
            return 0;
        return tableView.direction == TableView.Direction.Verical ? data[section].header.Rect.sizeDelta.y : data[section].header.Rect.sizeDelta.x;
    }

    public override int GetNumberOfRows(TableView tableView, int section)
    {
        return data[section].numberOfRows;
    }

    public override int GetNumberOfSections(TableView tableView)
    {
        return data !=  null ? data.Length : 0;
    }

    public override float GetRowHeight(TableView tableView, int section, int row)
    {
        return tableView.direction == TableView.Direction.Verical ? data[section].cell.Rect.sizeDelta.y : data[section].cell.Rect.sizeDelta.x;
    }

    public override float GetRowSpacing(TableView tableView, int section)
    {
        return data[section].spacing;
    }

    public override TableViewCell GetSectionHeader(TableView tableView, int section)
    {
        if (data[section].header == null)
            return null;
        
        var cell = tableView.GetReusableCell(data[section].header.reuseIdentifier);
        if (cell == null)
        {
            var go = Instantiate(data[section].header.gameObject) as GameObject;
            cell = go.GetComponent<TableViewCell>();
        }

        cell.gameObject.name = cell.GetComponentInChildren<Text>().text = string.Format("Header {0}", section);
        return cell;
    }

    [System.Serializable]
    public class SectionData
    {
        public TableViewCell header;
        public TableViewCell cell;
        public int numberOfRows;
        public float spacing;
    }
}