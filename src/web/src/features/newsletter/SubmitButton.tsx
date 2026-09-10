import { useFormStatus } from 'react-dom';
import { Button } from '@/shared/ui';

export function SubmitButton(): React.JSX.Element {
  const { pending } = useFormStatus();

  return (
    <Button type="submit" disabled={pending} className="text-white">
      {pending ? 'Subscribing…' : 'Subscribe'}
    </Button>
  );
}
