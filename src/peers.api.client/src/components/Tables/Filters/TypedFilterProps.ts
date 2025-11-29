import type { Filter as GenericFilter } from './utils';

export type TypedFilterProps = {
    idx: number;
    filter: GenericFilter;
    onChange: (idx: number, filter: GenericFilter) => void;
    onRemove: (idx: number) => void;
}
