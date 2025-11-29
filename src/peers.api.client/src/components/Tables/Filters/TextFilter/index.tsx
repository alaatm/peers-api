import type { FilterDropdownProps } from 'antd/es/table/interface';
import { BaseFilter } from '@/components/Tables';
import Filter from './Filter';

/*
 * selectedKeys in this component will be following this pattern:
 * ['startswith/contains/endswith/eq/ne(value1)', 'or/and', 'startswith/contains/endswith/eq/ne(value1)', 'and/or', ...]
 *
 * ops and opnds arrays should always have equal length.
 * logicOps array should always be one less than the above arrays.
 */

const TextFilter = (props: FilterDropdownProps) => <BaseFilter {...props} TypedFilter={Filter} />;
export default TextFilter;
