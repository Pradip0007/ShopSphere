import type { OrderAddress } from './types';

interface AddressBlockProps {
  address: OrderAddress;
}

export function AddressBlock({ address }: AddressBlockProps): React.JSX.Element {
  return (
    <address className="text-sm not-italic text-[var(--color-text-muted)]">
      {address.fullName && (
        <>
          {address.fullName}
          <br />
        </>
      )}
      {address.line1}
      <br />
      {address.line2 && (
        <>
          {address.line2}
          <br />
        </>
      )}
      {address.city}, {address.postalCode}
      <br />
      {address.country}
      {address.phone && (
        <>
          <br />
          {address.phone}
        </>
      )}
    </address>
  );
}
