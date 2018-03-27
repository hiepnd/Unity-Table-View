using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class TableView : MonoBehaviour
{
    public Direction direction;
    public AbstractTableViewDataSource datasource;

    LinkedList<TableViewCell> visibleCells = new LinkedList<TableViewCell>();
    Dictionary<string, LinkedList<TableViewCell>> reusableCells = new Dictionary<string, LinkedList<TableViewCell>>();

    ScrollRect scrollRect;
    Transform reusableContainer;
    TableSize tableSize;

    public float TopOffset
    {
        get {
            var contentRect = scrollRect.content.rect;
            var viewportRect = scrollRect.viewport.rect;

            if (direction == Direction.Vertical) {
                var viewportMax = viewportRect.max;
                viewportMax = scrollRect.viewport.TransformPoint(viewportMax);
                viewportMax = scrollRect.content.InverseTransformPoint(viewportMax);

                return contentRect.max.y - viewportMax.y;
            } else {
                var viewportMin = viewportRect.min;
                viewportMin = scrollRect.viewport.TransformPoint(viewportMin);
                viewportMin = scrollRect.content.InverseTransformPoint(viewportMin);

                return -contentRect.min.x + viewportMin.x;
            }
        }
    }

    public float BottomOffset
    {
        get { 
            var contentRect = scrollRect.content.rect;
            var viewportRect = scrollRect.viewport.rect;

            if (direction == Direction.Vertical)
            {
                var viewportMin = viewportRect.min;
                viewportMin = scrollRect.viewport.TransformPoint(viewportMin);
                viewportMin = scrollRect.content.InverseTransformPoint(viewportMin);

                return contentRect.max.y - viewportMin.y;
            }
            else
            {
                var viewportMax = viewportRect.max;
                viewportMax = scrollRect.viewport.TransformPoint(viewportMax);
                viewportMax = scrollRect.content.InverseTransformPoint(viewportMax);

                return viewportMax.x - contentRect.min.x;
            }
        }
    }

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        reusableContainer = new GameObject("Reusable Cells Container").transform;
        reusableContainer.SetParent(transform);
    }

    private IEnumerator Start()
    {
        yield return null;
        ReloadData();
    }

    void OnEnable()
    {
        scrollRect.onValueChanged.AddListener(OnScrolled);
    }

    void OnDisable()
    {
        scrollRect.onValueChanged.RemoveListener(OnScrolled);
    }

    public void ReloadData(bool purgeCache = false)
    {
        if (datasource == null)
        {
            Debug.LogError("No data source");
            return;
        }

        if (purgeCache)
        {
            foreach (var i in reusableCells) {
                while (i.Value.Count > 0) {
                    Destroy(i.Value.First.Value.gameObject);
                    i.Value.RemoveFirst();
                }
            }
            reusableCells.Clear();

            while (visibleCells.Count > 0)
            {
                Destroy(visibleCells.First.Value.gameObject);
                visibleCells.RemoveFirst();
            }
        }
        else
        {
            // Try to cache visible cells
            while (visibleCells.Count > 0)
            {
                var cell = visibleCells.First.Value;
                if (!Cache(cell))
                {
                    Destroy(cell.gameObject);
                }

                visibleCells.RemoveFirst();
            }
        }

        visibleCells.Clear();

        tableSize = new TableSize(this, datasource);
        var contentSize = scrollRect.content.sizeDelta;
        if (direction == Direction.Vertical) {
            contentSize.y = tableSize.TotalHeight;
        } else {
            contentSize.x = tableSize.TotalHeight;
        }
        scrollRect.content.sizeDelta = contentSize;

        GenerateCells();
    }

    void OnScrolled(Vector2 d)
    {
        if (datasource == null || tableSize == null || tableSize.Empty)
            return;

        RemoveTopInvisible();
        RemoveBottomInvisible();

        if (visibleCells.Count == 0)
        {
            GenerateCells();
        }
        else
        {
            InsertTopCells();
            InserBottomCells();
        }
    }

    private void RemoveTopInvisible ()
    {
        if (visibleCells.Count == 0)
            return;

        int section = 0, row = 0;
        if (tableSize.RowAt(TopOffset, ref section, ref row))
        {
            while (visibleCells.Count > 0 && TableSize.IndexCompare(section, row, visibleCells.First.Value.Section, visibleCells.First.Value.Row) > 0) {
                Cache(visibleCells.First.Value);
                visibleCells.RemoveFirst();
            }
        }
    }

    private void RemoveBottomInvisible()
    {
        if (visibleCells.Count == 0)
            return;

        int section = 0, row = 0;
        if (tableSize.RowAt(BottomOffset, ref section, ref row))
        {
            while (visibleCells.Count > 0 && TableSize.IndexCompare(section, row, visibleCells.Last.Value.Section, visibleCells.Last.Value.Row) < 0)
            {
                Cache(visibleCells.Last.Value);
                visibleCells.RemoveLast();
            }
        }
    }

    private void GenerateCells()
    {
        int startSection = 0, startRow = 0;
        int endSection = 0, endRow = 0;

        if (tableSize.FindCellRange(TopOffset, BottomOffset, ref startSection, ref startRow, ref endSection, ref endRow))
        {
            GenerateCells(startSection, startRow, endSection, endRow);
        }
        else
        {
            Debug.Log("No cell");
        }
    }

    private delegate bool Traverse (ref int x, ref int y, int z, int t) ; 
    private void GenerateCells(int startSection, int startRow, int endSection, int endRow, bool backward=false)
    {
        if (backward)
            startRow++;
        else
            startRow--;

        Traverse traverse = tableSize.Next;
        if (backward) traverse = tableSize.Prev;

        while (traverse(ref startSection, ref startRow, endSection, endRow))
        {
            if (startRow == -1)
            {
                // Header
                var header = datasource.GetSectionHeader(this, startSection);
                if (header != null)
                {
                    header.SetIndex(startSection, startRow);
                    AddCell(header, tableSize.Offset(startSection, startRow + 1), backward);
                }
            }
            else
            {
                var cell = datasource.GetCell(this, startSection, startRow);
                cell.SetIndex(startSection, startRow);
                AddCell(cell, tableSize.Offset(startSection, startRow + 1), backward);
            }
        }
    }

    private void InserBottomCells()
    {
        int startSection = visibleCells.Last.Value.Section, startRow= visibleCells.Last.Value.Row;
        int endSection = 0, endRow = 0;
        if (!tableSize.RowAt(BottomOffset, ref endSection, ref endRow))
        {
            endSection = tableSize.SectionCount - 1;
            endRow = tableSize.LastSection.RowCount - 1;
        }

        if (TableSize.IndexCompare(startSection, startRow, endSection, endRow) < 0)
        {
            tableSize.Next(ref startSection, ref startRow, endSection, endRow);
            GenerateCells(startSection, startRow, endSection, endRow);
        }
    }

    private void InsertTopCells()
    {
        int startSection = 0, startRow = 0;
        int endSection = visibleCells.First.Value.Section, endRow = visibleCells.First.Value.Row;

        if (!tableSize.RowAt(TopOffset, ref startSection, ref startRow))
        {
            startSection = 0;
            endSection = -1;
        }

        if (TableSize.IndexCompare(startSection, startRow, endSection, endRow) < 0)
        {
            tableSize.Prev(ref endSection, ref endRow, startSection, startRow);
            GenerateCells(endSection, endRow, startSection, startRow, true);
        }
    }

    private void AddCell(TableViewCell cell, float offset, bool atFirst)
    {
        if (direction == Direction.Vertical)
            offset = -offset;
        var rect = cell.Rect;
        rect.SetParent(scrollRect.content, false);
        rect.localScale = Vector3.one;
        if (direction == Direction.Vertical) {
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, offset);    
        } else {
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(offset, 0);
        }

        if (atFirst)
            visibleCells.AddFirst(cell);
        else
            visibleCells.AddLast(cell);

    }

    public TableViewCell GetReusableCell(string identifier)
    {
        LinkedList<TableViewCell> cells;
        if (reusableCells.TryGetValue(identifier, out cells))
        {
            if (cells.Count > 0)
            {
                var cell = cells.Last.Value;
                cells.RemoveLast();
                cell.gameObject.SetActive(true);
                return cell;
            }
        }

        return null;
    }

    private bool Cache(TableViewCell cell)
    {
        if (string.IsNullOrEmpty(cell.reuseIdentifier))
        {
            return false;
        }

        LinkedList<TableViewCell> cells;
        if (!reusableCells.TryGetValue(cell.reuseIdentifier, out cells))
        {
            cells = new LinkedList<TableViewCell>();
            reusableCells[cell.reuseIdentifier] = cells;
        }

        cell.transform.SetParent(reusableContainer);
        cell.gameObject.SetActive(false);
        cells.AddLast(cell);
        return true;
    }

   
    public int NumberOfSections
    {
        get { return datasource.GetNumberOfSections(this); }
    }

    public int NumberOfRows(int section)
    {
        return datasource.GetNumberOfRows(this, section);
    }

    private class TableSize
    {
        readonly SectionSize[] sections;

        public TableSize(TableView table, AbstractTableViewDataSource datasource)
        {
            sections = new SectionSize[datasource.GetNumberOfSections(table)];
            float offset = 0;
            Empty = true;
            for (int i = 0; i < sections.Length; i++)
            {
                var section = new SectionSize(table, datasource, i, offset);
                sections[i] = section;
                offset = section.Bottom;
                Empty = Empty && section.Empty;
            }
        }

        public float TotalHeight
        {
            get { return sections[sections.Length - 1].Bottom; }
        }

        public bool Empty
        {
            get;
            private set;
        }

        public int SectionCount
        {
            get { return sections.Length; }
        }

        public SectionSize FirstSection
        {
            get { return sections[0]; }
        }

        public SectionSize LastSection
        {
            get { return sections[sections.Length - 1]; }
        }

        public int SectionAt(float offset)
        {
            if (offset < 0 || offset > TotalHeight)
                return -1;

            int i = sections.Length - 1;
            while (sections[i].Top > offset)
                i--;

            return i;
        }

        public bool RowAt(float offset, ref int section, ref int row)
        {
            section = SectionAt(offset);
            if (section >= 0)
                row = sections[section].RowAt(offset);

            return row >= -1 && section >= 0;
        }

        public bool Next(ref int currentSection, ref int currentRow, int stopSection, int stopRow)
        {
            var section = sections[currentSection];
            if (!section.Next(ref currentRow))
            {
                // No more row in current section
                if (currentSection >= sections.Length - 1)
                {
                    // No more section
                    return false;
                }
                else
                {
                    // Next section
                    currentSection++;
                    currentRow = -1;
                    return IndexCompare(currentSection, currentRow, stopSection, stopRow) <= 0;
                }
            }
            else
            {
                return IndexCompare(currentSection, currentRow, stopSection, stopRow) <= 0;
            }
        }

        public bool Prev(ref int currentSection, ref int currentRow, int stopSection, int stopRow)
        {
            var section = sections[currentSection];
            if (!section.Prev(ref currentRow)) {
                if (currentSection == 0) {
                    return false;
                } else {
                    currentSection--;
                    currentRow = sections[currentSection].RowCount - 1;
                    return IndexCompare(currentSection, currentRow, stopSection, stopRow) >= 0;
                }
            } else {
                return IndexCompare(currentSection, currentRow, stopSection, stopRow) >= 0;
            }
        }

        public bool FindCellRange(float startOffset, float endOffset, ref int startSection, ref int startRow, ref int endSection, ref int endRow)
        {
            if (Empty || endOffset < 0 || startOffset > TotalHeight)
            {
                return false;
            }

            if (!RowAt(startOffset, ref startSection, ref startRow))
            {
                startSection = 0;
                startRow = -1;
            }

            if (!RowAt(endOffset, ref endSection, ref endRow))
            {
                var lastSection = sections[sections.Length - 1];
                endSection = sections.Length - 1;
                endRow = lastSection.RowCount - 1;
            }

            return true;
        }

        public float Offset(int section, int index)
        {
            return sections[section].Offset(index);
        }

        public static int IndexCompare(int s0, int r0, int s1, int r1)
        {
            return s0 != s1 ? s0 - s1 : r0 - r1;
        }
    }

    private class SectionSize
    {
        readonly float[] rows;

        public SectionSize(TableView tableView, AbstractTableViewDataSource datasource, int section, float offset)
        {
            int rowCount = datasource.GetNumberOfRows(tableView, section);
            float headerHeight = datasource.GetHeaderHeight(tableView, section);
            float spacing = datasource.GetRowSpacing(tableView, section);

            rows = new float[rowCount + 1];
            rows[0] = offset;
            if (rowCount > 0) {
                rows[1] = rows[0] + headerHeight + spacing;
                for (int i = 2; i <= rowCount; i++)
                {
                    rows[i] = rows[i - 1] + datasource.GetRowHeight(tableView, section, i - 1) + spacing;
                }    
            }

            HasHeader = datasource.GetHeaderHeight(tableView, section) > 0;
            Top = rows[0];
            if (rowCount == 0)
                Bottom = rows[rows.Length - 1] + headerHeight + spacing;
            else
                Bottom = rows[rows.Length - 1] + datasource.GetRowHeight(tableView, section, rowCount - 1) + spacing;
        }

        public float Top
        {
            get;
            private set;
        }

        public float Bottom
        {
            get;
            private set;
        }

        public bool HasHeader
        {
            get;
            private set;
        }

        public int RowCount
        {
            get { return rows.Length - 1; }
        }

        public bool Empty
        {
            get
            {
                return rows.Length == 1 && Bottom <= Top;
            }
        }

        public float Offset(int index)
        {
            return rows[index];
        }

        /**
         * @return: -2 : no row or header, -1 : header
         **/
        public int RowAt(float offset)
        {
            if (offset < Top || offset > Bottom)
                return -2;

            int i = rows.Length - 1;
            while (rows[i] > offset)
                i--;
            return i - 1;
        }

        public bool Next(ref int index)
        {
            if (index < rows.Length - 2)
            {
                index++;
                return true;
            }
            return false;
        }

        public bool Prev(ref int index)
        {
            if (index > -1) {
                index--;
                return true;
            }
            return false;
        }
    }

    public enum Direction
    {
        Vertical,
        Horizontal
    }

    [ContextMenu("Reload Data")]
    void ReloadNoPurgeCache()
    {
        ReloadData(false);
    }

    [ContextMenu("Purge Cache and Reload Data")]
    void ReloadAndPurgeCache()
    {
        var pet = new { Age = 10, Name = "Fluffy" };  

        ReloadData(true);
    }

    [ContextMenu("Log Offset")]
    void LogOffset()
    {
        Debug.LogFormat("Top Offset {0}, Bottom Offset {1}", TopOffset, BottomOffset);

        int section = 0, row = 0;
        tableSize.RowAt(TopOffset, ref section, ref row);
        Debug.LogFormat("Cell at top ({0}, {1})", section, row);

        tableSize.RowAt(BottomOffset, ref section, ref row);
        Debug.LogFormat("Cell at bottom ({0}, {1})", section, row);
    }
}
