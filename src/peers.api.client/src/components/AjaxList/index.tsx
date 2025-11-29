import { type Key, useEffect, useState } from 'react';
import { type ListProps, List, type TablePaginationConfig } from 'antd';
import type { PaginationConfig } from 'antd/es/pagination';
import type { AnyObject } from 'antd/es/_util/type';
import { isOk, type PagedQueryResponse, type ProblemDetails } from '@/api';
import type { TableParams } from '../Tables/AjaxTable';
import './index.css';

export interface AjaxListProps<T, R extends PagedQueryResponse<T> = PagedQueryResponse<T>> extends ListProps<T> {
    fetchData: (params: TableParams<T>) => Promise<R | ProblemDetails>;
    onDataFetched?: (data: R) => void;
    reloadToken?: boolean;
    scrollOnPageChange?: boolean;
    initialSort?: { field: keyof T, order: 'ascend' | 'descend', };
}

const AjaxList = <T extends AnyObject, R extends PagedQueryResponse<T> = PagedQueryResponse<T>>({
    pagination,
    fetchData,
    onDataFetched,
    reloadToken,
    scrollOnPageChange = true,
    initialSort,
    ...rest
}: AjaxListProps<T, R>) => {
    const computedPagination = pagination === false
        ? undefined
        : { ...(pagination || {}), current: pagination?.current ?? 1, pageSize: pagination?.pageSize ?? 15 };
    reloadToken = reloadToken || false;
    const [loading, setLoading] = useState(false);
    const [data, setData] = useState<R>();
    const [tableParams, setTableParams] = useState<TableParams<T>>({
        pagination: computedPagination as TablePaginationConfig,
        sortField: initialSort?.field as Key | undefined,
        sortOrder: initialSort?.order,
        filters: undefined,
    });
    const [total, setTotal] = useState(0);
    const [currentPage, setCurrentPage] = useState<number>();

    useEffect(() => {
        setLoading(true);

        fetchData(tableParams).then((response) => {
            if (isOk(response)) {
                setData(response);
                setTotal(response.total);
                onDataFetched?.(response);
                setCurrentPage(tableParams.pagination!.current);
            }
            setLoading(false);
        });
        // fetchData & onDataFetched are dependecies but will likely never change
        // However, adding them to the dependencies, as adviced by react, will cause this effect to run
        // on every render or state change in the parent component.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [tableParams, /*fetchData, onDataFetched*/ reloadToken]);

    useEffect(() => {
        if (scrollOnPageChange && currentPage) {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    }, [scrollOnPageChange, currentPage]);

    const handleTableChange = (page: number, pageSize: number) => {
        setTableParams(prev => ({
            ...prev,
            pagination: {
                ...prev.pagination,
                current: page,
                pageSize: pageSize,
            },
        }));
    };

    return (
        <List<T>
            dataSource={data?.data}
            pagination={computedPagination === undefined
                ? undefined
                : {
                    align: 'start',
                    ...tableParams.pagination as PaginationConfig,
                    total,
                    onChange: handleTableChange,
                }}
            loading={loading}
            {...rest}
        />
    )
};

export default AjaxList;
