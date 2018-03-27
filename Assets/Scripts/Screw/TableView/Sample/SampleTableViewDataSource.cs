/****************************************************************************
 Copyright (c) hiepndhut@gmail.com
 Copyright (c) 2018 No PowerUp
 
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
 ****************************************************************************/

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
        return tableView.direction == TableView.Direction.Vertical ? data[section].header.Rect.sizeDelta.y : data[section].header.Rect.sizeDelta.x;
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
        return tableView.direction == TableView.Direction.Vertical ? data[section].cell.Rect.sizeDelta.y : data[section].cell.Rect.sizeDelta.x;
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