import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'descriptionLength',
})
export class DescriptionLengthPipe implements PipeTransform {
  transform(value: string | null | undefined, maxlength: number): string {
    if (typeof maxlength !== 'number') {
      return value ?? '';
    }
    return (value?.length ?? 0) > maxlength ? `${value?.substring(0, maxlength)}...` : value ?? '';
  }
}
