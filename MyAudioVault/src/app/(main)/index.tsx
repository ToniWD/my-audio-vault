import { View, Text, StyleSheet, Button } from 'react-native';
import { useAuth } from '../../hooks/useAuth';
import { Colors } from '@/constants/colors';
import { globalStyles } from '@/constants/globalStyles';
import { RoleLabels } from '@/utils/roleLabels';
import { router } from 'expo-router';
import { useEffect, useState } from 'react';

export default function MainIndex() {
    const { user } = useAuth();

    useEffect(() => {

    }, []);

    return (
        <View style={[globalStyles.screen, globalStyles.center]}>
            <Text style={globalStyles.title}>MyAudioVault</Text>
            <Text style={globalStyles.subtitle}>Welcome, {user?.firstName ?? user?.email}</Text>
            <Text style={[globalStyles.caption, styles.role]}>Role: {user ? RoleLabels[user.role] : 'Unknown'}</Text>
            <Button title='Test Play' onPress={() => {
                router.push('/audioPlayer')
            }} />
        </View>
    );
}

const styles = StyleSheet.create({
    role: {
        color: Colors.mainTheme.textMuted,
        marginBottom: 32,
    },
});
