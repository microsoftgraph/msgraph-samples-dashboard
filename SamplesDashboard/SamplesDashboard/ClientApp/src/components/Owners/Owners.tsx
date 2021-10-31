// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Link } from '@fluentui/react';
import RepoOwner from '../../types/RepoOwner';

interface OwnersProps {
  owners: RepoOwner[];
}

export default function Owners(props: OwnersProps) {
  return (
    <ul>
      {props.owners?.map(function (owner: RepoOwner, index: number) {
        return (
          <li style={{ listStyle: 'none' }} key={index}>
            <Link href={owner.url} target='_blank' rel='noreferrer'>
              {owner.displayName}
            </Link>
          </li>
        );
      })}
    </ul>
  );
}
