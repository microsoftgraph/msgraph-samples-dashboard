// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { FontSizes, Pivot, PivotItem } from '@fluentui/react';
import { RouteComponentProps } from 'react-router-dom';
import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from '@azure/msal-react';

import Repositories from '../Repositories/Repositories';

export default function Home(props: RouteComponentProps) {
  return (
    <div>
      <AuthenticatedTemplate>
        <Pivot
          linkSize='large'
          styles={{
            text: { fontSize: FontSizes.xLarge },
            root: { marginBottom: '20px' },
          }}
          defaultSelectedKey='1'
        >
          <PivotItem headerText='Samples' itemKey='1'>
            <Repositories type='samples' />
          </PivotItem>
          <PivotItem headerText='SDKs' itemKey='2'>
            <Repositories type='sdks' />
          </PivotItem>
        </Pivot>
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <div>Please sign in</div>
      </UnauthenticatedTemplate>
    </div>
  );
}
