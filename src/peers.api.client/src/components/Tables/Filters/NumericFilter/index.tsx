import type { FilterDropdownProps } from 'antd/es/table/interface';
import { BaseFilter } from '@/components/Tables';
import Filter from './Filter';

/*
 * selectedKeys in this component will be following this pattern:
 * ['lt/le/eq/ge/gt/ne(value1)', 'or/and', 'lt/le/eq/ge/gt/ne(value1)', 'and/or', ...]
 *
 * ops and opnds arrays should always have equal length.
 * logicOps array should always be one less than the above arrays.
 */

const NumericFilter = (props: FilterDropdownProps) => <BaseFilter {...props} TypedFilter={Filter} />;
export default NumericFilter;
