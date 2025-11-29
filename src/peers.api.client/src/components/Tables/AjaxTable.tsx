import { useEffect, useState } from 'react';
import { type TablePaginationConfig, type GetProp, Table, type TableProps } from 'antd';
import type { ColumnType, FilterValue, SorterResult } from 'antd/es/table/interface';
import type { AnyObject } from 'antd/es/_util/type';
import { useLang } from '@/Language';
import { isOk, type PagedQueryResponse, type ProblemDetails } from '@/api';
import './AjaxTable.css';

export interface ColumnTypeEx<T = AnyObject> extends ColumnType<T> {
    editable?: boolean;
    filterKey?: string;
};

export type ColumnsTypeEx<T> = ColumnTypeEx<T>[];

export type TableParams<T = AnyObject> = {
    pagination?: TablePaginationConfig;
    sortField?: SorterResult<T>['field'];
    sortOrder?: SorterResult<T>['order'];
    filters?: Parameters<GetProp<AjaxTableProps, 'onChange'>>[1];
}

export interface AjaxTableProps<T = AnyObject, R extends PagedQueryResponse<T> = PagedQueryResponse<T>> extends TableProps<T> {
    saving?: boolean;
    initialSort?: SorterResult<T>;
    initialFilters?: Record<string, FilterValue | null>;
    columns: ColumnsTypeEx<T>;
    fetchData: (params: TableParams<T>) => Promise<R | ProblemDetails>;
    onDataFetched?: (data: R) => void;
    reloadToken?: boolean;
    scrollOnPageChange?: boolean;
};

const AjaxTable = <T extends AnyObject, R extends PagedQueryResponse<T> = PagedQueryResponse<T>>({
    saving,
    pagination,
    initialSort,
    initialFilters,
    columns,
    fetchData,
    onDataFetched,
    reloadToken,
    scrollOnPageChange = true,
    ...rest }: AjaxTableProps<T, R>) => {
    const { lang: { dir } } = useLang();
    const computedPagination = pagination === false
        ? undefined
        : { ...(pagination || {}), current: pagination?.current ?? 1, pageSize: pagination?.pageSize ?? 15 };
    const [loading, setLoading] = useState(false);
    const [data, setData] = useState<R>();
    const [tableParams, setTableParams] = useState<TableParams<T>>({
        pagination: computedPagination,
        sortField: Array.isArray(initialSort) ? undefined : initialSort?.field,
        sortOrder: Array.isArray(initialSort) ? undefined : initialSort?.order,
        filters: initialFilters,
    });
    const [total, setTotal] = useState(0);
    const [currentPage, setCurrentPage] = useState<number>();

    // Update `tableParams` when `initialFilters` changes
    useEffect(() => {
        setTableParams(prev => ({
            ...prev,
            filters: initialFilters,
        }));
    }, [initialFilters]);

    useEffect(() => {
        setLoading(true);

        // Transform filters:
        // - Use `filterKey` instead of `dataIndex` as the key if `filterKey` is defined on the column.
        // - Remove filters with `null` values.
        const transformedFilters = Object.entries(tableParams.filters || {})
            .filter(([, value]) => value !== null)
            .reduce((acc, [key, value]) => {
                const column = columns.find((col) => col.dataIndex === key);
                acc[column?.filterKey || key] = value as FilterValue;
                return acc;
            }, {} as Record<string, FilterValue>);

        const params = {
            ...tableParams,
            filters: Object.keys(transformedFilters).length ? transformedFilters : undefined,
        };

        fetchData(params).then((response) => {
            if (isOk(response)) {
                setData(response);
                setTotal(response.total);
                onDataFetched?.(response);
                setCurrentPage(tableParams.pagination!.current);
            }
            setLoading(false);
        });
        // columns, fetchData & onDataFetched are dependecies but will likely never change
        // However, adding them to the dependencies, as adviced by react, will cause this effect to run
        // on every render or state change in the parent component.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [tableParams, /*columns, fetchData, onDataFetched,*/ reloadToken]);

    useEffect(() => {
        if (scrollOnPageChange && currentPage) {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    }, [scrollOnPageChange, currentPage]);

    const handleTableChange: TableProps<T>['onChange'] = (pagination, filters, sorter) => {
        const currSortField = Array.isArray(sorter) ? undefined : sorter.field;
        const currSortOrder = Array.isArray(sorter) ? undefined : sorter.order;
        const sortField = currSortField ?? (Array.isArray(initialSort) ? undefined : initialSort?.field);
        const sortOrder = currSortOrder ?? (Array.isArray(initialSort) ? undefined : initialSort?.order);

        setTableParams({
            pagination,
            sortField,
            sortOrder,
            filters: filters ?? initialFilters,
        });
    };

    return (
        <Table<T>
            bordered
            columns={columns}
            dataSource={data?.data}
            pagination={computedPagination === undefined
                ? undefined
                : {
                    position: dir === 'rtl'
                        ? ['bottomRight']
                        : ['bottomLeft'],
                    ...tableParams.pagination,
                    total,
                }}
            loading={loading || saving}
            onChange={handleTableChange}
            {...rest}
        />
    )
};

export default AjaxTable;
