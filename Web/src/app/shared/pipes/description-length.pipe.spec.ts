import { DescriptionLengthPipe } from './description-length.pipe';

describe('DescriptionLengthPipe', () => {
  it('create an instance', () => {
    const pipe = new DescriptionLengthPipe();
    expect(pipe).toBeTruthy();
  });
});
